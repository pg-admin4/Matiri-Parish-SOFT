using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using Microsoft.Data.Sqlite;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class SyncService
    {
        private readonly string _dbPath;
        private readonly HttpClient _http;
        private readonly SemaphoreSlim _syncLock = new(1,1);

        public SyncService(HttpClient httpClient)
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "matiri_parish.db");
            _http = httpClient;
            EnsureTables();

            // Start processing when connectivity changes to Online
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            _ = ProcessQueueIfConnectedAsync();
        }

        private void EnsureTables()
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS PendingSync(
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    EntityType TEXT NOT NULL,
                                    EntityId INTEGER NOT NULL,
                                    Attempts INTEGER NOT NULL DEFAULT 0,
                                    LastError TEXT,
                                    CreatedUtc TEXT NOT NULL
                                );";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE INDEX IF NOT EXISTS IX_PendingSync_EntityType ON PendingSync(EntityType);";
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public Task EnqueueEntitySyncAsync(string entityType, int entityId)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO PendingSync(EntityType, EntityId, CreatedUtc) VALUES($type,$id,$created);";
                cmd.Parameters.AddWithValue("$type", entityType);
                cmd.Parameters.AddWithValue("$id", entityId);
                cmd.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }
            catch { }
            _ = ProcessQueueIfConnectedAsync();
            return Task.CompletedTask;
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
                await ProcessQueueIfConnectedAsync();
        }

        private async Task ProcessQueueIfConnectedAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;
            if (!await _syncLock.WaitAsync(0)) return;
            try
            {
                while (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    var item = DequeueNextPending();
                    if (item == null) break;
                    bool ok = await TrySyncItemAsync(item);
                    if (!ok)
                    {
                        // stop processing to avoid tight loop; backoff handled per-item
                        break;
                    }
                }
            }
            finally
            {
                _syncLock.Release();
            }
        }

        private PendingItem? DequeueNextPending()
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var tran = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, EntityType, EntityId, Attempts FROM PendingSync ORDER BY CreatedUtc LIMIT 1;";
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;
                var item = new PendingItem
                {
                    Id = reader.GetInt32(0),
                    EntityType = reader.GetString(1),
                    EntityId = reader.GetInt32(2),
                    Attempts = reader.GetInt32(3)
                };
                return item;
            }
            catch { return null; }
        }

        private async Task<bool> TrySyncItemAsync(PendingItem item)
        {
            try
            {
                // Build payload by reading local EF SQLite DB for the entity
                string endpoint = item.EntityType switch
                {
                    "FirstHolyCommunionRecord" => "api/firstholycommunion",
                    "ConfirmationRecord" => "api/confirmation",
                    "BaptismRecord" => "api/baptism",
                    "MarriageRecord" => "api/marriage",
                    "DeathRegistryRecord" => "api/deathregistry",
                    _ => null
                };

                if (endpoint == null) { await MarkFailedAsync(item.Id, "Unknown entity type"); return true; }

                // Load entity JSON from local DB using simple query
                string json = await BuildEntityJsonAsync(item.EntityType, item.EntityId);
                if (json == null)
                {
                    await DeletePendingAsync(item.Id); // nothing to sync
                    return true;
                }

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var res = await _http.PostAsync(endpoint, content);
                if (res.IsSuccessStatusCode)
                {
                    await DeletePendingAsync(item.Id);
                    return true;
                }
                else if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // let AuthMessageHandler attempt refresh on next run; mark attempt
                    await IncrementAttemptAsync(item.Id);
                    return false;
                }
                else
                {
                    await IncrementAttemptAsync(item.Id);
                    return false;
                }
            }
            catch
            {
                await IncrementAttemptAsync(item.Id);
                return false;
            }
        }

        private Task<string?> BuildEntityJsonAsync(string entityType, int entityId)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                if (entityType == "FirstHolyCommunionRecord")
                {
                    cmd.CommandText = "SELECT PersonId, DateOfFirstCommunion, OfficiatingPriest, PrayerHouse, BookNumber, PageNumber, EntryNumber, Remarks FROM FirstHolyCommunions WHERE Id = $id LIMIT 1;";
                }
                else if (entityType == "ConfirmationRecord")
                {
                    cmd.CommandText = "SELECT PersonId, DateOfConfirmation, OfficiatingBishop, SponsorName, PrayerHouse, BookNumber, PageNumber, EntryNumber FROM Confirmations WHERE Id = $id LIMIT 1;";
                }
                else if (entityType == "BaptismRecord")
                {
                    cmd.CommandText = "SELECT PersonId, DateOfBaptism, OfficiatingPriest, GodfatherName, GodmotherName, BookNumber, PageNumber, EntryNumber, Remarks, PrayerHouse FROM Baptisms WHERE Id = $id LIMIT 1;";
                }
                else if (entityType == "MarriageRecord")
                {
                    cmd.CommandText = "SELECT GroomId, BrideId, DateOfMarriage, Priest, Witness1, Witness2, PrayerHouse, BookNumber, PageNumber, EntryNumber FROM Marriages WHERE Id = $id LIMIT 1;";
                }
                else if (entityType == "DeathRegistryRecord")
                {
                    cmd.CommandText = "SELECT PersonId, DateOfDeath, PlaceOfDeath, CauseOfDeath, InformantName, BookNumber, PageNumber, EntryNumber, Remarks FROM DeathRegistries WHERE Id = $id LIMIT 1;";
                }
                else return Task.FromResult<string?>(null);

                cmd.Parameters.AddWithValue("$id", entityId);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return Task.FromResult<string?>(null);
                var dict = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var val = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    dict[name] = val;
                }

                var json = JsonSerializer.Serialize(dict);
                return Task.FromResult<string?>(json);
            }
            catch { return Task.FromResult<string?>(null); }
        }

        private async Task DeletePendingAsync(int id)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM PendingSync WHERE Id = $id;";
                cmd.Parameters.AddWithValue("$id", id);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        private async Task IncrementAttemptAsync(int id)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE PendingSync SET Attempts = Attempts + 1, LastError = $err WHERE Id = $id;";
                cmd.Parameters.AddWithValue("$id", id);
                cmd.Parameters.AddWithValue("$err", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        private async Task MarkFailedAsync(int id, string message)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE PendingSync SET Attempts = Attempts + 1, LastError = $err WHERE Id = $id;";
                cmd.Parameters.AddWithValue("$id", id);
                cmd.Parameters.AddWithValue("$err", message);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        private class PendingItem { public int Id; public string EntityType = string.Empty; public int EntityId; public int Attempts; }
    }
}

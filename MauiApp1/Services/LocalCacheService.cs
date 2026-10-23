using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using MauiApp1.Models;
using Microsoft.Data.Sqlite;

namespace MauiApp1.Services
{
    public class LocalCacheService
    {
        private readonly string _dbPath;

        public LocalCacheService()
        {
            // Use the same Core EF SQLite database file to keep cache co-located with app data
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "matiri_parish.db");
            Initialize();
        }

        private void Initialize()
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS CachedPerson(
                                Id INTEGER PRIMARY KEY,
                                FirstName TEXT,
                                MiddleName TEXT,
                                LastName TEXT,
                                DateOfBirth TEXT,
                                FatherName TEXT,
                                MotherName TEXT,
                                BaptismBook TEXT,
                                BaptismPage INTEGER,
                                BaptismEntry INTEGER,
                                LastSeenUtc TEXT
                            );";
            cmd.ExecuteNonQuery();

            // Create indexes to speed up name queries
            cmd.CommandText = "CREATE INDEX IF NOT EXISTS IX_CachedPerson_LastName_FirstName ON CachedPerson(LastName, FirstName);";
            cmd.ExecuteNonQuery();
        }

        public Task UpsertPersonsAsync(IEnumerable<PersonSummaryDto> persons)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var tran = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT OR REPLACE INTO CachedPerson(Id, FirstName, MiddleName, LastName, DateOfBirth, FatherName, MotherName, BaptismBook, BaptismPage, BaptismEntry, LastSeenUtc)
                                    VALUES($id,$first,$middle,$last,$dob,$father,$mother,$book,$page,$entry,$seen);";

                foreach (var p in persons)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$id", p.Id);
                    cmd.Parameters.AddWithValue("$first", p.FirstName ?? string.Empty);
                    cmd.Parameters.AddWithValue("$middle", p.MiddleName ?? string.Empty);
                    cmd.Parameters.AddWithValue("$last", p.LastName ?? string.Empty);
                    cmd.Parameters.AddWithValue("$dob", p.DateOfBirth.ToString("o"));
                    cmd.Parameters.AddWithValue("$father", p.FatherName ?? string.Empty);
                    cmd.Parameters.AddWithValue("$mother", p.MotherName ?? string.Empty);
                    cmd.Parameters.AddWithValue("$book", p.BaptismBook ?? string.Empty);
                    cmd.Parameters.AddWithValue("$page", p.BaptismPage);
                    cmd.Parameters.AddWithValue("$entry", p.BaptismEntry);
                    cmd.Parameters.AddWithValue("$seen", DateTime.UtcNow.ToString("o"));
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                // best-effort caching — swallow errors
            }

            return Task.CompletedTask;
        }

        public Task<List<PersonSummaryDto>> QueryByNameAsync(string first, string last, int limit = 50, int offset = 0)
        {
            var results = new List<PersonSummaryDto>();
            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT Id, FirstName, MiddleName, LastName, DateOfBirth, FatherName, MotherName, BaptismBook, BaptismPage, BaptismEntry
                                    FROM CachedPerson
                                    WHERE FirstName LIKE $first AND LastName LIKE $last
                                    ORDER BY LastName, FirstName
                                    LIMIT $limit OFFSET $offset;";
                cmd.Parameters.AddWithValue("$first", first + "%");
                cmd.Parameters.AddWithValue("$last", last + "%");
                cmd.Parameters.AddWithValue("$limit", limit);
                cmd.Parameters.AddWithValue("$offset", offset);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var p = new PersonSummaryDto
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        MiddleName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        LastName = reader.GetString(3),
                        DateOfBirth = DateTime.TryParse(reader.GetString(4), out var dt) ? dt : DateTime.MinValue,
                        FatherName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        MotherName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        BaptismBook = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        BaptismPage = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                        BaptismEntry = reader.IsDBNull(9) ? 0 : reader.GetInt32(9)
                    };
                    results.Add(p);
                }
            }
            catch
            {
                // ignore
            }

            return Task.FromResult(results);
        }
    }
}

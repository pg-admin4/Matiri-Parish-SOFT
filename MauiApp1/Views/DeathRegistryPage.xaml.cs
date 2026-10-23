using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using MauiApp1.Data;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.Infrastructure;

namespace MauiApp1.Views;

public partial class DeathRegistryPage : ContentPage
{
    private Person? _verifiedPerson;

    public DeathRegistryPage()
    {
        InitializeComponent();
    }

    private System.Threading.CancellationTokenSource? _debounceCts;

    private void LookupFullNameEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        LookupButton.IsEnabled = false;
        _debounceCts?.Cancel();
        _debounceCts = new System.Threading.CancellationTokenSource();
        var token = _debounceCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(600, token);
                if (!token.IsCancellationRequested)
                {
                    await Dispatcher.DispatchAsync(() => LookupButton.IsEnabled = true);
                }
            }
            catch (TaskCanceledException) { }
        });
    }

    private async void OnLookupClicked(object sender, EventArgs e)
    {
        string fullName = LookupFullNameEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fullName))
        {
            await DisplayAlert("Lookup Error", "Please enter a full name to verify.", "OK");
            return;
        }

        string[] parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            await DisplayAlert("Lookup Error", "Please enter both first and last name.", "OK");
            return;
        }

        string first = parts[0];
        string last = parts[parts.Length - 1];

        // Use paged selection modal
        var selectionPage = new PersonSelectionPage("baptism", first, last);
        await Navigation.PushModalAsync(selectionPage);
        var selected = await selectionPage.WaitForSelectionAsync();
        if (selected == null)
        {
            SaveButton.IsEnabled = false;
            await DisplayAlert("Cannot proceed", "Cannot proceed. Person not found in the Baptism register. Please register Baptism first.", "OK");
            return;
        }

        _verifiedPerson = new Person { Id = selected.Id, FirstName = selected.FirstName, MiddleName = selected.MiddleName, LastName = selected.LastName, DateOfBirth = selected.DateOfBirth };

        FirstNameEntry.Text = _verifiedPerson.FirstName;
        MiddleNameEntry.Text = _verifiedPerson.MiddleName;
        LastNameEntry.Text = _verifiedPerson.LastName;
        FirstNameEntry.IsEnabled = false;
        MiddleNameEntry.IsEnabled = false;
        LastNameEntry.IsEnabled = false;
        SaveButton.IsEnabled = true;
        StatusLabel.Text = "Verified from Baptism register.";
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            using var db = new MatiriParishDbContext();

            int? personId = null;

            if (_verifiedPerson != null)
            {
                // server-side guard: ensure Baptism exists for this person
                var person = await db.Persons.Include(p => p.Baptism).FirstOrDefaultAsync(p => p.Id == _verifiedPerson.Id && p.Baptism != null);
                if (person == null)
                {
                    throw new ValidationException("Cannot proceed. Person not found in the Baptism register. Please register Baptism first.");
                }

                personId = person.Id;
            }

            var record = new DeathRegistryRecord
            {
                PersonId = personId,
                DateOfDeath = DateOfDeathPicker?.Date ?? DateTime.Today,
                PlaceOfDeath = PlaceOfDeathEntry.Text ?? string.Empty,
                CauseOfDeath = CauseOfDeathEntry.Text ?? string.Empty,
                InformantName = InformantEntry.Text ?? string.Empty,
                BookNumber = BookNumberEntry.Text ?? string.Empty,
                PageNumber = int.TryParse(PageNumberEntry.Text, out int page) ? page : 0,
                EntryNumber = int.TryParse(EntryNumberEntry.Text, out int entry) ? entry : 0,
                Remarks = RemarksEntry.Text ?? string.Empty
            };

            db.DeathRegistries.Add(record);
            await db.SaveChangesAsync();

            try
            {
                var sync = ServiceLocator.ServiceProvider?.GetService(typeof(MauiApp1.Services.SyncService)) as MauiApp1.Services.SyncService;
                if (sync != null)
                {
                    await sync.EnqueueEntitySyncAsync(nameof(DeathRegistryRecord), record.Id);
                }
            }
            catch { }

            StatusLabel.Text = "Death record saved successfully.";
            await DisplayAlert("Success", "Record saved successfully!", "OK");
            ClearForm();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Database Error", $"Failed to save record: {ex.Message}", "OK");
        }
    }

    private void ClearForm()
    {
        FirstNameEntry.Text = string.Empty;
        LastNameEntry.Text = string.Empty;
        DateOfDeathPicker.Date = DateTime.Today;
        PlaceOfDeathEntry.Text = string.Empty;
        CauseOfDeathEntry.Text = string.Empty;
        InformantEntry.Text = string.Empty;
        BookNumberEntry.Text = string.Empty;
        PageNumberEntry.Text = string.Empty;
        EntryNumberEntry.Text = string.Empty;
        RemarksEntry.Text = string.Empty;
    }
}

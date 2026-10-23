using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using MauiApp1.Data;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.Infrastructure;

namespace MauiApp1.Views;

public partial class MarriagePage : ContentPage
{
    private Person? _verifiedGroom;
    private Person? _verifiedBride;

    public MarriagePage()
    {
        InitializeComponent();
    }

    private System.Threading.CancellationTokenSource? _groomDebounceCts;
    private System.Threading.CancellationTokenSource? _brideDebounceCts;

    private void GroomLookupEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        GroomLookupButton.IsEnabled = false;
        _groomDebounceCts?.Cancel();
        _groomDebounceCts = new System.Threading.CancellationTokenSource();
        var token = _groomDebounceCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(600, token);
                if (!token.IsCancellationRequested)
                {
                    await Dispatcher.DispatchAsync(() => GroomLookupButton.IsEnabled = true);
                }
            }
            catch (TaskCanceledException) { }
        });
    }

    private void BrideLookupEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        BrideLookupButton.IsEnabled = false;
        _brideDebounceCts?.Cancel();
        _brideDebounceCts = new System.Threading.CancellationTokenSource();
        var token = _brideDebounceCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(600, token);
                if (!token.IsCancellationRequested)
                {
                    await Dispatcher.DispatchAsync(() => BrideLookupButton.IsEnabled = true);
                }
            }
            catch (TaskCanceledException) { }
        });
    }

#pragma warning disable CA1416 // Platform compatibility
    private async void OnLookupGroomClicked(object sender, EventArgs e)
    {
        string fullName = GroomLookupEntry.Text?.Trim() ?? string.Empty;
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

        // Use paged selection modal for Confirmation lookup
        var selectionPage = new PersonSelectionPage("confirmation", first, last);
        await Navigation.PushModalAsync(selectionPage);
        var selected = await selectionPage.WaitForSelectionAsync();
        if (selected == null)
        {
            return;
        }

        _verifiedGroom = new Person { Id = selected.Id, FirstName = selected.FirstName, MiddleName = selected.MiddleName, LastName = selected.LastName, DateOfBirth = selected.DateOfBirth };

        GroomFirstNameEntry.Text = _verifiedGroom.FirstName;
        GroomMiddleNameEntry.Text = _verifiedGroom.MiddleName;
        GroomLastNameEntry.Text = _verifiedGroom.LastName;
        GroomFirstNameEntry.IsEnabled = false;
        GroomMiddleNameEntry.IsEnabled = false;
        GroomLastNameEntry.IsEnabled = false;
        StatusLabel.Text = "Groom verified from Confirmation register.";
    }

    private async void OnLookupBrideClicked(object sender, EventArgs e)
    {
        string fullName = BrideLookupEntry.Text?.Trim() ?? string.Empty;
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

        // Use paged selection modal for Confirmation lookup
        var selectionPage2 = new PersonSelectionPage("confirmation", first, last);
        await Navigation.PushModalAsync(selectionPage2);
        var selectedBride = await selectionPage2.WaitForSelectionAsync();
        if (selectedBride == null)
        {
            return;
        }

        _verifiedBride = new Person { Id = selectedBride.Id, FirstName = selectedBride.FirstName, MiddleName = selectedBride.MiddleName, LastName = selectedBride.LastName, DateOfBirth = selectedBride.DateOfBirth };

        BrideFirstNameEntry.Text = _verifiedBride.FirstName;
        BrideMiddleNameEntry.Text = _verifiedBride.MiddleName;
        BrideLastNameEntry.Text = _verifiedBride.LastName;
        BrideFirstNameEntry.IsEnabled = false;
        BrideMiddleNameEntry.IsEnabled = false;
        BrideLastNameEntry.IsEnabled = false;
        StatusLabel.Text = "Bride verified from Confirmation register.";
    }
#pragma warning restore CA1416

#pragma warning disable CA1416 // Platform compatibility
    private async void OnSaveMarriageClicked(object sender, EventArgs e)
    {
        // Cast sender once at the top to avoid variable scoping conflicts (CS0136)
        var saveButton = sender as Button;
        if (saveButton != null)
        {
            saveButton.IsEnabled = false;
        }

        var validator = new ParishValidationService();

        try
        {
            // Sanitize user inputs upfront
            string groomFirstName = GroomFirstNameEntry.Text?.Trim() ?? string.Empty;
            string groomLastName = GroomLastNameEntry.Text?.Trim() ?? string.Empty;
            string brideFirstName = BrideFirstNameEntry.Text?.Trim() ?? string.Empty;
            string brideLastName = BrideLastNameEntry.Text?.Trim() ?? string.Empty;

            // 1. Construct temporary Groom & Bride check objects
            var groomCheck = new Person
            {
                FirstName = groomFirstName,
                LastName = groomLastName,
                DateOfBirth = DateTime.Today
            };

            var brideCheck = new Person
            {
                FirstName = brideFirstName,
                LastName = brideLastName,
                DateOfBirth = DateTime.Today
            };

            // 2. Validate input fields for both
            validator.ValidatePerson(groomCheck);
            validator.ValidatePerson(brideCheck);

            using var db = new MatiriParishDbContext();

            // 3. Locate Groom or Create Record
            // Use invariant lowercase on the input values to avoid repeated ToLower calls in the query
            string groomFirstLower = groomFirstName.ToLowerInvariant();
            string groomLastLower = groomLastName.ToLowerInvariant();

            var groom = await db.Persons.FirstOrDefaultAsync(p =>
                p.FirstName.ToLower() == groomFirstLower &&
                p.LastName.ToLower() == groomLastLower);

            if (groom == null)
            {
                groom = new Person
                {
                    FirstName = groomFirstName,
                    LastName = groomLastName,
                    DateOfBirth = DateTime.Today
                };
                db.Persons.Add(groom);
                await db.SaveChangesAsync();
            }

            // 4. Locate Bride or Create Record
            string brideFirstLower = brideFirstName.ToLowerInvariant();
            string brideLastLower = brideLastName.ToLowerInvariant();

            var bride = await db.Persons.FirstOrDefaultAsync(p =>
                p.FirstName.ToLower() == brideFirstLower &&
                p.LastName.ToLower() == brideLastLower);

            if (bride == null)
            {
                bride = new Person
                {
                    FirstName = brideFirstName,
                    LastName = brideLastName,
                    DateOfBirth = DateTime.Today
                };
                db.Persons.Add(bride);
                await db.SaveChangesAsync();
            }

            // 5. Construct Marriage Record
            var marriage = new MarriageRecord
            {
                GroomId = groom.Id,
                BrideId = bride.Id,
                // Date is non-nullable on DatePicker; guard against null reference to the control itself
                DateOfMarriage = MarriageDatePicker?.Date ?? DateTime.Today,
                OfficiatingPriest = PriestEntry.Text?.Trim() ?? string.Empty,
                BestMan = Witness1Entry.Text?.Trim() ?? string.Empty,
                MaidOfHonor = Witness2Entry.Text?.Trim() ?? string.Empty,
                PrayerHouse = PrayerHouseEntry?.Text?.Trim() ?? string.Empty,
                BookNumber = BookNumberEntry.Text?.Trim() ?? "1",
                PageNumber = int.TryParse(PageNumberEntry.Text, out int page) ? page : 1,
                EntryNumber = int.TryParse(EntryNumberEntry.Text, out int entry) ? entry : 1
            };

            // 6. Validate Marriage rules (future date check & same-person check)
            validator.ValidateMarriage(marriage, groom, bride);

            // 7. Save Marriage Record
            db.Marriages.Add(marriage);
            await db.SaveChangesAsync();

            try
            {
                var sync = ServiceLocator.ServiceProvider?.GetService(typeof(MauiApp1.Services.SyncService)) as MauiApp1.Services.SyncService;
                if (sync != null)
                {
                    await sync.EnqueueEntitySyncAsync(nameof(MarriageRecord), marriage.Id);
                }
            }
            catch { }

            // 8. Success Output & Reset Form
            StatusLabel.Text = $"Successfully registered Marriage for {groom.FirstName} & {bride.FirstName}!";
            await DisplayAlert("Success", "Marriage record saved successfully!", "OK");
            ClearForm();
        }
        catch (ValidationException ex)
        {
            await DisplayAlert("Validation Error", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Database Error", $"Failed to save record: {ex.Message}", "OK");
        }
        finally
        {
            // Re-enable button cleanly using top-level reference
            if (saveButton != null)
            {
                saveButton.IsEnabled = true;
            }
        }
    }
#pragma warning restore CA1416

    private void ClearForm()
    {
        GroomFirstNameEntry.Text = string.Empty;
        GroomLastNameEntry.Text = string.Empty;
        BrideFirstNameEntry.Text = string.Empty;
        BrideLastNameEntry.Text = string.Empty;
        PriestEntry.Text = string.Empty;
        Witness1Entry.Text = string.Empty;
        Witness2Entry.Text = string.Empty;
        if (PrayerHouseEntry != null)
            PrayerHouseEntry.Text = string.Empty;
        BookNumberEntry.Text = string.Empty;
        PageNumberEntry.Text = string.Empty;
        EntryNumberEntry.Text = string.Empty;
        MarriageDatePicker.Date = DateTime.Today;
    }
}
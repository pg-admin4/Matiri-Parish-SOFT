using System;
using Microsoft.Maui.Controls;
using MauiApp1.Data;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.Infrastructure;

namespace MauiApp1.Views;

public partial class BaptismPage : ContentPage
{
    public BaptismPage()
    {
        InitializeComponent();
    }

    private async void OnSaveBaptismClicked(object sender, EventArgs e)
    {
        var validator = new ParishValidationService();

        try
        {
            using var db = new MatiriParishDbContext();

            // 1. Construct temporary Person check object
            var personCheck = new Person
            {
                FirstName = FirstNameEntry.Text ?? string.Empty,
                MiddleName = MiddleNameEntry?.Text ?? string.Empty,
                LastName = LastNameEntry.Text ?? string.Empty,
                DateOfBirth = DobPicker?.Date ?? DateTime.Today
            };

            // 2. Validate input fields
            validator.ValidatePerson(personCheck);

            // 3. Construct BaptismRecord object (Includes Jumuiya)
            var newRecord = new BaptismRecord
            {
                FirstName = personCheck.FirstName,
                LastName = personCheck.LastName,
                DateOfBirth = personCheck.DateOfBirth,
                PlaceOfBirth = PlaceOfBirthEntry?.Text ?? string.Empty,
                Jumuiya = JumuiyaEntry?.Text ?? string.Empty,
                PrayerHouse = PrayerHouseEntry?.Text ?? string.Empty,
                FatherName = FatherNameEntry?.Text ?? string.Empty,
                MotherName = MotherNameEntry?.Text ?? string.Empty,
                BaptismDate = BaptismDatePicker?.Date ?? DateTime.Today,
                Minister = OfficiatingPriestEntry?.Text ?? string.Empty,
                Godparent1 = Godparent1Entry?.Text ?? string.Empty,
                Godparent2 = Godparent2Entry?.Text ?? string.Empty,
                BookNumber = BookNumberEntry?.Text ?? string.Empty,
                PageNumber = int.TryParse(PageNumberEntry?.Text, out int page) ? page : 0,
                EntryNumber = int.TryParse(EntryNumberEntry?.Text, out int entry) ? entry : 0
            };

            // 4. Validate canonical Baptism rules
            validator.ValidateBaptism(newRecord, personCheck);

            // 5. Check for duplicate person using named arguments to prevent parameter type mismatch
            bool isDuplicate = await validator.IsDuplicatePersonAsync(
                db: db,
                firstName: personCheck.FirstName,
                lastName: personCheck.LastName,
                dateOfBirth: personCheck.DateOfBirth,
                middleName: personCheck.MiddleName
            );

            if (isDuplicate)
            {
                bool proceed = await DisplayAlert(
                    "Possible Duplicate Found",
                    $"A member named '{personCheck.FullName}' born on {personCheck.DateOfBirth:yyyy-MM-dd} already exists in the system.\n\nDo you still want to save this baptism record?",
                    "Yes, Save",
                    "Cancel"
                );

                if (!proceed) return;
            }

            // 6. Save to Database
            db.Baptisms.Add(newRecord);
            await db.SaveChangesAsync();

            await DisplayAlert("Success", "Baptism record saved successfully!", "OK");

            try
            {
                var sync = ServiceLocator.ServiceProvider?.GetService(typeof(MauiApp1.Services.SyncService)) as MauiApp1.Services.SyncService;
                if (sync != null)
                {
                    await sync.EnqueueEntitySyncAsync(nameof(BaptismRecord), newRecord.Id);
                }
            }
            catch { }

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
    }

    private void ClearForm()
    {
        FirstNameEntry.Text = string.Empty;

        if (MiddleNameEntry != null)
            MiddleNameEntry.Text = string.Empty;

        LastNameEntry.Text = string.Empty;

        if (PlaceOfBirthEntry != null)
            PlaceOfBirthEntry.Text = string.Empty;

        if (JumuiyaEntry != null)
            JumuiyaEntry.Text = string.Empty;

        if (PrayerHouseEntry != null)
            PrayerHouseEntry.Text = string.Empty;

        FatherNameEntry.Text = string.Empty;
        MotherNameEntry.Text = string.Empty;
        OfficiatingPriestEntry.Text = string.Empty;
        Godparent1Entry.Text = string.Empty;
        Godparent2Entry.Text = string.Empty;
        BookNumberEntry.Text = string.Empty;
        PageNumberEntry.Text = string.Empty;
        EntryNumberEntry.Text = string.Empty;
    }
}
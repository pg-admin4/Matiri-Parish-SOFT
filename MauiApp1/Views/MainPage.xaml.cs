using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using MauiApp1.Data;

namespace MauiApp1.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardStatsAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadDashboardStatsAsync();
    }

    private async Task LoadDashboardStatsAsync()
    {
        try
        {
            // Offload database queries to a background thread to prevent blocking the WinUI window creation pump
            var (personCount, baptismCount, confirmationCount, marriageCount) = await Task.Run(async () =>
            {
                using var db = new MatiriParishDbContext();
                int pCount = await db.Persons.CountAsync();
                int bCount = await db.Baptisms.CountAsync();
                int cCount = await db.Confirmations.CountAsync();
                int mCount = await db.Marriages.CountAsync();

                return (pCount, bCount, cCount, mCount);
            });

            // Update UI on main thread safely
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (TotalPersonsCard != null)
                    TotalPersonsCard.ValueText = personCount.ToString();
                if (TotalBaptismsCard != null)
                    TotalBaptismsCard.ValueText = baptismCount.ToString();
                if (TotalConfirmationsCard != null)
                    TotalConfirmationsCard.ValueText = confirmationCount.ToString();
                if (TotalMarriagesCard != null)
                    TotalMarriagesCard.ValueText = marriageCount.ToString();
            });
        }
        catch (Exception ex)
        {
            // Prevent silent crashes during window instantiation
            System.Diagnostics.Debug.WriteLine($"Error loading stats: {ex.Message}");
        }
    }

    private async void OnGoToSearchClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SearchPage");
    }

    private async void OnGoToConfirmationClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ConfirmationPage");
    }

    private async void OnGoToMarriageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MarriagePage");
    }
}
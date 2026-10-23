using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using MauiApp1.Data;
using MauiApp1.Models;

namespace MauiApp1.Views;

public partial class SearchPage : ContentPage
{
    public SearchPage()
    {
        InitializeComponent();
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        string query = SearchEntry.Text?.Trim() ?? string.Empty;

        using var db = new MatiriParishDbContext();

        // Query database for persons matching first, middle, or last name
        var persons = await db.Persons
            .Include(p => p.Baptism)
            .Include(p => p.Confirmation)
            .Include(p => p.MarriageAsGroom)
            .Include(p => p.MarriageAsBride)
            .Where(p => string.IsNullOrWhiteSpace(query) ||
                        p.FirstName.Contains(query) ||
                        p.MiddleName.Contains(query) ||
                        p.LastName.Contains(query))
            .ToListAsync();

        if (persons.Count == 0)
        {
            StatusLabel.Text = "No records found matching your search.";
            ResultsCollectionView.ItemsSource = null;
            return;
        }

        StatusLabel.Text = $"Found {persons.Count} record(s).";

        // Map database records into display models retaining the Person ID
        var displayResults = persons.Select(p => new PersonSearchResultViewModel
        {
            PersonId = p.Id,
            FullName = string.IsNullOrWhiteSpace(p.MiddleName)
                ? $"{p.FirstName} {p.LastName}".Trim()
                : $"{p.FirstName} {p.MiddleName} {p.LastName}".Trim(),
            DateOfBirthText = $"DOB: {p.DateOfBirth:dd MMM yyyy}",
            PlaceOfBirthText = $"Born in: {(string.IsNullOrEmpty(p.PlaceOfBirth) ? "N/A" : p.PlaceOfBirth)}",
            BaptismInfo = p.Baptism != null
                ? $"Baptized: {p.Baptism.DateOfBaptism:dd MMM yyyy} | Book #{p.Baptism.BookNumber}, Page #{p.Baptism.PageNumber}, Entry #{p.Baptism.EntryNumber}"
                : "No Baptism Record On File"
        }).ToList();

        ResultsCollectionView.ItemsSource = displayResults;
    }

    // Selection event for navigation when a user taps a record in CollectionView
    private async void OnPersonSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is PersonSearchResultViewModel selectedItem)
        {
            // Clear selection highlight
            ((CollectionView)sender).SelectedItem = null;

            // Navigate to PersonProfilePage passing the PersonId query parameter
            await Shell.Current.GoToAsync($"{nameof(SearchPage)}?PersonId={selectedItem.PersonId}");
        }
    }
}

// Helper class for UI binding and ID tracking
public class PersonSearchResultViewModel
{
    public int PersonId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DateOfBirthText { get; set; } = string.Empty;
    public string PlaceOfBirthText { get; set; } = string.Empty;
    public string BaptismInfo { get; set; } = string.Empty;
}
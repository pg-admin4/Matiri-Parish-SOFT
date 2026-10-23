using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls;

namespace MauiApp1.Views;

public partial class PersonSelectionPage : ContentPage
{
    private readonly TaskCompletionSource<PersonSummaryDto?> _tcs = new();
    private readonly ObservableCollection<PersonSummaryDto> _items = new();
    private readonly PersonSearchService _searchService;
    private readonly string _first;
    private readonly string _last;
    private readonly string _type;
    private int _page = 1;
    private const int PageSize = 25;
    private bool _isLoading = false;
    private long _totalCount = 0;

    public PersonSelectionPage(string type, string first, string last)
    {
        InitializeComponent();
        _first = first;
        _last = last;
        _type = type;

        MatchesCollection.ItemsSource = _items;
        MatchesCollection.SelectionChanged += MatchesCollection_SelectionChanged;
        CancelButton.Clicked += CancelButton_Clicked;

        _searchService = new PersonSearchService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Load data safely when page is fully rendered
        if (_items.Count == 0)
        {
            await LoadNextPageAsync();
        }
    }

    private async Task LoadNextPageAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            PagedResult<PersonSummaryDto> pageResult = _type.ToLower() switch
            {
                "baptism" => await _searchService.SearchBaptismPagedAsync(_first, _last, _page, PageSize),
                "firstholycommunion" => await _searchService.SearchFirstHolyCommunionPagedAsync(_first, _last, _page, PageSize),
                "confirmation" => await _searchService.SearchConfirmationPagedAsync(_first, _last, _page, PageSize),
                _ => new PagedResult<PersonSummaryDto> { Page = _page, PageSize = PageSize, TotalCount = 0 }
            };

            _totalCount = pageResult.TotalCount;

            foreach (var it in pageResult.Items)
            {
                _items.Add(it);
            }

            TotalLabel.Text = $"Showing {_items.Count} of {_totalCount}";
            TotalLabel.IsVisible = true;
            NoResultsLabel.IsVisible = (_items.Count == 0);
            LoadMoreButton.IsVisible = _items.Count < _totalCount;

            if (_items.Count < _totalCount)
            {
                _page++;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Search Error", $"Failed to load matches: {ex.Message}", "OK");
        }
        finally
        {
            _isLoading = false;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void MatchesCollection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.Count > 0 ? e.CurrentSelection[0] as PersonSummaryDto : null;
        if (item != null)
        {
            _tcs.TrySetResult(item);
            await Navigation.PopModalAsync();
        }
    }

    private async void CancelButton_Clicked(object? sender, EventArgs e)
    {
        _tcs.TrySetResult(null);
        await Navigation.PopModalAsync();
    }

    private async void MatchesCollection_RemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        if (_items.Count >= _totalCount) return;
        await LoadNextPageAsync();
    }

    private async void LoadMoreButton_Clicked(object? sender, EventArgs e)
    {
        await LoadNextPageAsync();
    }

    public Task<PersonSummaryDto?> WaitForSelectionAsync() => _tcs.Task;
}
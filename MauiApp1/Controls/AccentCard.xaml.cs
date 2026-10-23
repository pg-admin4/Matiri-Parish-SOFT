using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MauiApp1.Controls;

public partial class AccentCard : ContentView
{
    // Accent color for the left bar and frame border
    public static readonly BindableProperty AccentColorProperty = BindableProperty.Create(
        nameof(AccentColor),
        typeof(Color),
        typeof(AccentCard),
        Color.FromArgb("#5C0B1E")
    );

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    // Light theme card background
    public static readonly BindableProperty CardBackgroundLightProperty = BindableProperty.Create(
        nameof(CardBackgroundLight),
        typeof(Color),
        typeof(AccentCard),
        Color.FromArgb("#FFF6E6"),
        propertyChanged: OnCardBackgroundChanged
    );

    public Color CardBackgroundLight
    {
        get => (Color)GetValue(CardBackgroundLightProperty);
        set => SetValue(CardBackgroundLightProperty, value);
    }

    // Dark theme card background
    public static readonly BindableProperty CardBackgroundDarkProperty = BindableProperty.Create(
        nameof(CardBackgroundDark),
        typeof(Color),
        typeof(AccentCard),
        Color.FromArgb("#0F172A"),
        propertyChanged: OnCardBackgroundChanged
    );

    public Color CardBackgroundDark
    {
        get => (Color)GetValue(CardBackgroundDarkProperty);
        set => SetValue(CardBackgroundDarkProperty, value);
    }

    // The actual background color exposed to XAML for binding
    public static readonly BindableProperty CardBackgroundColorProperty = BindableProperty.Create(
        nameof(CardBackgroundColor),
        typeof(Color),
        typeof(AccentCard),
        Color.FromArgb("#FFF6E6")
    );

    public Color CardBackgroundColor
    {
        get => (Color)GetValue(CardBackgroundColorProperty);
        private set => SetValue(CardBackgroundColorProperty, value);
    }

    public AccentCard()
    {
        InitializeComponent();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        // Safely hook theme changes only after the control is attached to the visual tree
        if (Parent != null)
        {
            UpdateCardBackgroundColor();
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
                Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
            }
        }
        else
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
            }
        }
    }

    private static void OnCardBackgroundChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AccentCard card)
        {
            card.UpdateCardBackgroundColor();
        }
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateCardBackgroundColor);
    }

    private void UpdateCardBackgroundColor()
    {
        var theme = Application.Current?.RequestedTheme ?? AppTheme.Unspecified;
        CardBackgroundColor = (theme == AppTheme.Dark) ? CardBackgroundDark : CardBackgroundLight;
    }
}
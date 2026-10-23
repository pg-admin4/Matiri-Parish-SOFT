using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MauiApp1.Controls;

public partial class StatsCard : ContentView
{
    public static readonly BindableProperty AccentColorProperty = BindableProperty.Create(
        nameof(AccentColor), typeof(Color), typeof(StatsCard), Colors.Transparent);

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public static readonly BindableProperty IconBackgroundColorProperty = BindableProperty.Create(
        nameof(IconBackgroundColor), typeof(Color), typeof(StatsCard), Colors.Transparent);

    public Color IconBackgroundColor
    {
        get => (Color)GetValue(IconBackgroundColorProperty);
        set => SetValue(IconBackgroundColorProperty, value);
    }

    // Typed as View for safe XAML layout parsing
    public static readonly BindableProperty IconContentProperty = BindableProperty.Create(
        nameof(IconContent), typeof(View), typeof(StatsCard), null);

    public View? IconContent
    {
        get => (View?)GetValue(IconContentProperty);
        set => SetValue(IconContentProperty, value);
    }

    public static readonly BindableProperty CardBackgroundColorProperty = BindableProperty.Create(
        nameof(CardBackgroundColor), typeof(Color), typeof(StatsCard), Colors.Transparent);

    public Color CardBackgroundColor
    {
        get => (Color)GetValue(CardBackgroundColorProperty);
        set => SetValue(CardBackgroundColorProperty, value);
    }

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(StatsCard), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty ValueTextProperty = BindableProperty.Create(
        nameof(ValueText), typeof(string), typeof(StatsCard), "0");

    public string ValueText
    {
        get => (string)GetValue(ValueTextProperty);
        set => SetValue(ValueTextProperty, value);
    }

    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(StatsCard), string.Empty);

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public StatsCard()
    {
        InitializeComponent();
    }
}
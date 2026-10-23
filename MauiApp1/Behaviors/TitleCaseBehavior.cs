using MauiApp1.Services;

namespace MauiApp1.Behaviors
{
    public class TitleCaseBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.Unfocused += OnUnfocused;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.Unfocused -= OnUnfocused;
            base.OnDetachingFrom(entry);
        }

        private void OnUnfocused(object? sender, FocusEventArgs e)
        {
            if (sender is Entry entry && !string.IsNullOrWhiteSpace(entry.Text))
            {
                entry.Text = InputFormatter.ToTitleCase(entry.Text);
            }
        }
    }
}
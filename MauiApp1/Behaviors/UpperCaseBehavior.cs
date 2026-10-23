using System;
using Microsoft.Maui.Controls;
using MauiApp1.Services;

namespace MauiApp1.Behaviors
{
    public class UpperCaseBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnTextChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry && !string.IsNullOrEmpty(e.NewTextValue))
            {
                string upper = e.NewTextValue.ToUpper();
                if (entry.Text != upper)
                {
                    entry.Text = upper;
                }
            }
        }
    }
}
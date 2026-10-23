using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace MauiApp1.Services
{
    // Small debug helper to invoke a test alert from the debugger or instrumentation code.
    public static class DebugHelpers
    {
        public static async Task InvokeTestAlert(string title, string message)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    // Prefer Shell.Current when available
                    if (Shell.Current != null)
                    {
                        await Shell.Current.DisplayAlert(title, message, "OK");
                        return;
                    }

                    var page = Application.Current?.MainPage;
                    if (page != null)
                    {
                        await page.DisplayAlert(title, message, "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DebugHelpers.InvokeTestAlert failed: {ex}");
            }
        }
    }
}

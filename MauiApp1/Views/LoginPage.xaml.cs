using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using MauiApp1.Services;
using MatiriParish.Core.Services;
using CoreLoginStatus = MatiriParish.Core.Models.LoginStatus;
using Microsoft.EntityFrameworkCore;
using MatiriParish.Core.Data;
using MauiApp1.Infrastructure;
using System.Threading.Tasks;

namespace MauiApp1.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Redirect to admin registration if no admin exists (dev / first-run flow)
            var factory = ServiceLocator.ServiceProvider?.GetService(typeof(IDbContextFactory<MatiriParishDbContext>)) as IDbContextFactory<MatiriParishDbContext>;
            if (factory != null)
            {
                using var db = factory.CreateDbContext();
                bool hasAdmin = await db.Users.AnyAsync(u => u.Role == MatiriParish.Core.Models.UserRole.Admin);
                // Always show the register button (handled in XAML). If no admin exists prompt user before redirecting.
                if (!hasAdmin)
                {
                    try
                    {
                        bool create = await DisplayAlert("No admin found", "No admin account exists. Create admin now?", "Yes", "No");
                        if (create)
                        {
                            await Shell.Current.GoToAsync(nameof(AdminRegistrationPage));
                            return;
                        }
                        // user chose not to create admin now; continue showing login page
                    }
                    catch
                    {
                        // if DisplayAlert fails for any reason, fall back to not redirecting
                    }
                }
            }
        }
        catch
        {
            // ignore and stay on login page
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Direct access to controls via x:Name from XAML
        string username = UsernameEntry?.Text?.Trim() ?? string.Empty;
        string password = PasswordEntry?.Text ?? string.Empty;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetError(ErrorMessageLabel, "Please enter both username and password.");
            return;
        }

        try
        {
            // 1. Authenticate credentials asynchronously against SQLite database
            var result = await AuthService.Shared.LoginAsync(username, password);

            switch (result.Status)
            {
                case CoreLoginStatus.Success:
                    ClearError(ErrorMessageLabel);
                    // 2. Check if MFA is required for this account
                    bool isMfaRequired = await AuthService.Shared.IsMfaEnabledForUserAsync(username);

                    if (isMfaRequired)
                    {
                        // Route to MFA OTP Verification Page
                        await Shell.Current.GoToAsync($"//MfaVerificationPage?username={Uri.EscapeDataString(username)}");
                    }
                    else
                    {
                        // Route directly to main dashboard
                        await Shell.Current.GoToAsync("//MainPage");
                    }
                    break;

                case CoreLoginStatus.LockedOut:
                    SetError(ErrorMessageLabel, result.Message);
                    break;

                case CoreLoginStatus.Invalid:
                default:
                    SetError(ErrorMessageLabel, result.Message);
                    break;
            }
        }
        catch (Exception ex)
        {
            SetError(ErrorMessageLabel, $"Login error: {ex.Message}");
        }
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        // Route to Forgot Password Page
        // Use relative routing here because absolute ('//') routes require other pages on the stack.
        await Shell.Current.GoToAsync("ForgotPasswordPage");
    }

    private async void OnGoogleSignInClicked(object sender, EventArgs e)
    {
        try
        {
            var svc = new MauiApp1.Services.GoogleAuthService();
            await svc.StartInteractiveSignInAsync();
            // On success, you may want to route to main page or refresh UI
            await DisplayAlert("Signed in", "Google sign-in completed.", "OK");
        }
        catch (Exception ex)
        {
            SetError(ErrorMessageLabel, $"Sign-in error: {ex.Message}");
        }
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        try
        {
            if (PasswordEntry != null && TogglePasswordButton != null)
            {
                PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
                TogglePasswordButton.Text = PasswordEntry.IsPassword ? "Show" : "Hide";
            }
        }
        catch { }
    }

    private void SetError(Label? label, string message)
    {
        if (label != null)
        {
            label.Text = message;
            label.IsVisible = true;
        }
        else
        {
            // Ensure alert is shown on UI thread without blocking callers.
            _ = MainThread.InvokeOnMainThreadAsync(() =>
                Application.Current?.MainPage?.DisplayAlert("Login Error", message, "OK") ?? System.Threading.Tasks.Task.CompletedTask
            );
        }
    }

    private void ClearError(Label? label)
    {
        if (label != null)
        {
            label.IsVisible = false;
        }
    }

    private async void OnRegisterAdminClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(AdminRegistrationPage));
        }
        catch { }
    }
}
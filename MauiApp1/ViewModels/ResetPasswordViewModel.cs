using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiApp1.Services;
using MatiriParish.Core.Services;

namespace MauiApp1.ViewModels;

public class ResetPasswordViewModel : INotifyPropertyChanged
{
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;

    public string NewPassword
    {
        get => _newPassword;
        set { _newPassword = value; OnPropertyChanged(); }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set { _confirmPassword = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public ICommand ResetPasswordCommand { get; }

    public ResetPasswordViewModel()
    {
        ResetPasswordCommand = new Command(async () => await ExecuteResetAsync());
    }

    private async Task ExecuteResetAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        string username = ResetPasswordState.TargetUsername;

        // Hash using PBKDF2 and update SQLite DB via AuthService
        bool success = await AuthService.Shared.ResetPasswordAsync(username, NewPassword);

        if (success)
        {
            ResetPasswordState.TargetUsername = string.Empty;
            await Shell.Current.DisplayAlert("Success", "Password updated successfully. Please log in.", "OK");

            // Clear route stack back to login
            await Shell.Current.GoToAsync("//LoginPage");
        }
        else
        {
            ErrorMessage = "Failed to reset password. Please try again.";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
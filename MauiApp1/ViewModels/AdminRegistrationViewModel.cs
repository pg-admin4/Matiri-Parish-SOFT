using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiApp1.Services;
using MatiriParish.Core.Services;

namespace MauiApp1.ViewModels;

public class AdminRegistrationViewModel : INotifyPropertyChanged
{
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
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

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public ICommand RegisterAdminCommand { get; }

    public AdminRegistrationViewModel()
    {
        RegisterAdminCommand = new Command(async () => await RegisterAdminAsync());
    }

    private async Task RegisterAdminAsync()
    {
        if (IsBusy) return;

        ErrorMessage = string.Empty;

        // Validation checks
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please fill in all fields.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return;
        }

        IsBusy = true;

        try
        {
            // Register using the AuthService setup previously
            bool success = await AuthService.Shared.RegisterInitialAdminAsync(Username, Password);

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Admin account created successfully! Please log in.", "OK");

                // Navigate back to Login page
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                ErrorMessage = "An Admin account already exists or registration failed.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
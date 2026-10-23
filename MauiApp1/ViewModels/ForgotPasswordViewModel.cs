using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiApp1.Services;
using MatiriParish.Core.Services;
using MauiApp1.Views;

namespace MauiApp1.ViewModels;

public class ForgotPasswordViewModel : INotifyPropertyChanged
{
    private string _username = string.Empty;
    private string _errorMessage = string.Empty;

    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public ICommand SendOtpCommand { get; }

    public ForgotPasswordViewModel()
    {
        SendOtpCommand = new Command(async () => await SendOtpAsync());
    }

    private async Task SendOtpAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Please enter your username.";
            return;
        }

        // Generate cryptographically secure OTP via AuthService
        string otp = AuthService.Shared.GenerateOtp(Username);

        if (string.IsNullOrEmpty(otp))
        {
            ErrorMessage = "Too many OTP requests. Please try again later.";
            return;
        }

        // Store target user and navigate to MFA verification step
        ResetPasswordState.TargetUsername = Username.Trim();
        await Shell.Current.GoToAsync(nameof(MfaVerificationPage));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiApp1.Services;
using MauiApp1.Views;
using MatiriParish.Core.Services;

namespace MauiApp1.ViewModels;

public class MfaVerificationViewModel : INotifyPropertyChanged
{
    private string _otpCode = string.Empty;
    private string _errorMessage = string.Empty;

    public string OtpCode
    {
        get => _otpCode;
        set { _otpCode = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public ICommand VerifyOtpCommand { get; }

    public MfaVerificationViewModel()
    {
        VerifyOtpCommand = new Command(async () => await VerifyOtpAsync());
    }

    private async Task VerifyOtpAsync()
    {
        string username = ResetPasswordState.TargetUsername;

        if (string.IsNullOrWhiteSpace(OtpCode))
        {
            ErrorMessage = "Please enter the verification code.";
            return;
        }

        // Validate using constant-time comparison in AuthService
        bool isValid = AuthService.Shared.ValidateOtp(username, OtpCode);

        if (isValid)
        {
            // Valid code -> Proceed to Reset Password step
            await Shell.Current.GoToAsync(nameof(ResetPasswordPage));
        }
        else
        {
            ErrorMessage = "Invalid or expired verification code.";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
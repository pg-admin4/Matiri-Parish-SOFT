using MatiriParish.Core.Services;

namespace MauiApp1.Services;

// Backwards-compatible interface that forwards to the core IAuthService
public interface IAuthService : MatiriParish.Core.Services.IAuthService
{
}

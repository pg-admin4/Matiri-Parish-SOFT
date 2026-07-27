using MauiApp1.Data; // Directives belong at the top of the file
using Microsoft.Extensions.Logging;

namespace MauiApp1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // 1. Register DbContext in the dependency injection container
        builder.Services.AddDbContext<MatiriParishDbContext>();

        var app = builder.Build();

        // 2. Ensure database and tables are created automatically on launch
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MatiriParishDbContext>();
            dbContext.Database.EnsureCreated();
        }

        return app;
    }
}
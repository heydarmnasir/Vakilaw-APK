using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Plugin.LocalNotification;
using Vakilaw.Services;
using Vakilaw.ViewModels;
using Vakilaw.Views;

namespace Vakilaw;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        SQLitePCL.Batteries_V2.Init();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .ConfigureMopups()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("IRANSansWeb Persian.ttf", "IRANSansWeb");
                fonts.AddFont("Sahel.ttf", "Sahel");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // مسیر دیتابیس
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "vakilaw.db");

        // ثبت سرویس‌ها
        builder.Services.AddSingleton(s => new DatabaseService(dbPath));
        builder.Services.AddSingleton<LawDatabase>();
        builder.Services.AddSingleton<LawImporter>();
        builder.Services.AddSingleton<UserService>();

        // ویومدل‌ها
        builder.Services.AddSingleton<MainPageVM>();
        builder.Services.AddSingleton<LawBankVM>();

        // صفحات
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<LawBankPage>();

        builder.Services.AddSingleton<App>();

        return builder.Build();
    }
}
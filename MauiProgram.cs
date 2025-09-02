using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Plugin.LocalNotification;
using System.Runtime.InteropServices;
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
        string dbPath = GetDatabasePath();

        // ثبت سرویس‌ها
        builder.Services.AddSingleton(new DatabaseService(dbPath));
        builder.Services.AddSingleton<UserService>();
        builder.Services.AddTransient<MainPageVM>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LawyerSubmitPopup>();
        builder.Services.AddSingleton<App>();

        return builder.Build();
    }

    private static string GetDatabasePath()
    {
        string dbFileName = "vakilaw.db";
        string folder;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        else
        {
#if ANDROID
            folder = FileSystem.AppDataDirectory;
#elif IOS
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#else
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
        }

        return Path.Combine(folder, dbFileName);
    }
}
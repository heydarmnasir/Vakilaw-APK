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
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "vakilaw.db");

        // -------------------- سرویس‌ها --------------------
        builder.Services.AddSingleton(s => new DatabaseService(dbPath));
        builder.Services.AddSingleton<LawService>(s => new LawService(s.GetRequiredService<DatabaseService>()));
        builder.Services.AddSingleton<LawImporter>(s => new LawImporter(s.GetRequiredService<LawService>()));
        builder.Services.AddSingleton<UserService>();
        builder.Services.AddSingleton<OtpService>();
        builder.Services.AddSingleton(sp =>
        new LicenseService(sp.GetRequiredService<DatabaseService>(), "<PUBLIC_KEY_BASE64_HERE>"));
        builder.Services.AddSingleton<LawyerService>(s => new LawyerService(s.GetRequiredService<DatabaseService>()));

        builder.Services.AddSingleton<ClientService>();
        builder.Services.AddSingleton<CaseService>();

        // -------------------- ویومدل‌ها --------------------
        builder.Services.AddSingleton<MainPageVM>(); // Singleton برای حفظ داده‌ها
        builder.Services.AddSingleton<LawBankVM>();

        builder.Services.AddTransient<LawyerSubmitVM>();
        builder.Services.AddTransient<SubscriptionPopupVM>();


        builder.Services.AddTransient<ClientsAndCasesViewModel>();
     
        // -------------------- صفحات --------------------
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LawBankPage>();
        builder.Services.AddTransient<LawyerSubmitPopup>();
        builder.Services.AddTransient<SubscriptionPopup>();

        builder.Services.AddTransient<ClientsAndCasesPage>();    

        // -------------------- خود App --------------------
        builder.Services.AddSingleton<App>();

        return builder.Build();
    }



//    private static string GetDatabasePath()
//    {
//        string dbFileName = "TaskList.db";
//        string folder;

//        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//        {
//            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//        }
//        else
//        {
//#if ANDROID
//                folder = FileSystem.AppDataDirectory;
//#elif IOS
//            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//#else
//                folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//#endif
//        }

//        return Path.Combine(folder, dbFileName);
//    }
}
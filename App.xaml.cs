using Plugin.LocalNotification;
using Microsoft.Maui.Controls;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Storage;
using Vakilaw.Views;

namespace Vakilaw
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // گرفتن MainPage از DI
            //MainPage = services.GetRequiredService<MainPage>();
        }

        protected override void OnStart()
        {
            base.OnStart();

            // ⏳ نمایش نوتیفیکیشن خوش‌آمدگویی
            Task.Run(async () =>
            {
                var notification = new NotificationRequest
                {
                    NotificationId = 1000,
                    Title = "خوش آمد گویی",
                    Description = "به اپلیکیشن حقوقی وکیلاو خوش آمدید",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Now.AddSeconds(10)
                    }
                };

                await LocalNotificationCenter.Current.Show(notification);
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // اگر Shell نداریم و MainPage مستقیماً ContentPage است
            return new Window(new AppShell());
        }
    }
}
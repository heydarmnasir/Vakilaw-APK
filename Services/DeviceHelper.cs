namespace Vakilaw.Services
{
    public static class DeviceHelper
    {
        public static string GetDeviceId()
        {
            var id = Preferences.Get("DeviceId", string.Empty);
            if (string.IsNullOrWhiteSpace(id))
            {
                id = GenerateDeviceId();
            }
            return id;
        }

        private static string GenerateDeviceId()
        {
            var id = Guid.NewGuid().ToString("N"); // بدون خط تیره، فشرده‌تر
            Preferences.Set("DeviceId", id);
            return id;
        }
    }
}
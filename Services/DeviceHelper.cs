namespace Vakilaw.Services;
public static class DeviceHelper
{
    public static string GetDeviceId()
    {
        return Preferences.Get("DeviceId", null) ?? GenerateDeviceId();
    }

    private static string GenerateDeviceId()
    {
        var id = Guid.NewGuid().ToString();
        Preferences.Set("DeviceId", id);
        return id;
    }
}
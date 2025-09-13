namespace Vakilaw.Models
{
    internal class LicensePayload
    {
        public string DeviceId { get; set; } = string.Empty;
        public long StartTicks { get; set; }
        public long EndTicks { get; set; }
        public string SubscriptionType { get; set; } = string.Empty;
    }
}
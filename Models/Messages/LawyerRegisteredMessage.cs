using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Vakilaw.Models.Messages
{
    public class LawyerRegisteredMessage : ValueChangedMessage<string>
    {
        public string LicenseNumber { get; }
        public LawyerRegisteredMessage(string fullName, string licenseNumber) : base(fullName)
        {
            LicenseNumber = licenseNumber;
        }
    }
}
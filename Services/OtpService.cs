using IPE.SmsIrClient;
using IPE.SmsIrClient.Models.Requests;

public class OtpService
{
    private readonly SmsIr _smsIr;
    private readonly int _templateId = 188074; // 👈 اینجا TemplateId که از پنل گرفتی
    private readonly Random _random = new Random();

    public OtpService()
    {
        string apiKey = "nAhAuhG1zeatLYl8giJtgTgTc9L1788EQbckz7iGd1uYUz28"; // 👈 اینجا کلید API خودت
        _smsIr = new SmsIr(apiKey);
    }

    public async Task<string> SendOtpAsync(string phoneNumber, string fullName)
    {
        // تولید کد ۵ رقمی
        string otpCode = _random.Next(10000, 99999).ToString();

        // تنظیم پارامترهای قالب
        var parameters = new[]
        {
            new VerifySendParameter("VERIFICATIONCODE", otpCode),
            new VerifySendParameter("LAWYER", fullName),
        };

        // ارسال با استفاده از قالب
        var result = await _smsIr.VerifySendAsync(phoneNumber, _templateId, parameters);

        if (result == null || result.Status != 1)
        {
            throw new Exception($"خطا در ارسال کد تایید: {result?.Message ?? "Unknown error"}");
        }

        return otpCode;
    }
}
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Vakilaw.Models;

namespace Vakilaw.Services;

public class LicenseService
{
    private readonly DatabaseService _databaseService;
    private readonly string _publicKeyXml;

    public LicenseService(DatabaseService databaseService, string publicKeyXml)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _publicKeyXml = publicKeyXml ?? throw new ArgumentNullException(nameof(publicKeyXml));
    }

    public async Task<bool> ActivateSignedLicenseAsync(string signedLicense)
    {
        if (string.IsNullOrWhiteSpace(signedLicense))
            throw new InvalidOperationException("لایسنس خالی است.");

        // حذف همه فاصله‌ها، newline و tab
        signedLicense = new string(signedLicense.Where(c => !char.IsWhiteSpace(c)).ToArray());

        var parts = signedLicense.Split('.');
        if (parts.Length != 2)
            throw new InvalidOperationException("لایسنس ناقص است یا جداکننده '.' ندارد.");

        byte[] payloadBytes;
        byte[] signature;
        try
        {
            payloadBytes = Convert.FromBase64String(parts[0]);
            signature = Convert.FromBase64String(parts[1]);
        }
        catch
        {
            throw new InvalidOperationException("Base64 payload یا signature معتبر نیست.");
        }

        // بررسی امضا با PublicKey XML
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.PersistKeyInCsp = false;
            try
            {
                rsa.FromXmlString(LicenseConfig.PublicKeyXml);
            }
            catch
            {
                throw new InvalidOperationException("کلید عمومی XML معتبر نیست.");
            }

            if (!rsa.VerifyData(payloadBytes, CryptoConfig.MapNameToOID("SHA256"), signature))
            {
                string payloadText = Encoding.UTF8.GetString(payloadBytes);
                throw new InvalidOperationException($"امضا نامعتبر است! payload={payloadText}");
            }
        }

        // پارس payload
        LicensePayload? payload;
        try
        {
            var json = Encoding.UTF8.GetString(payloadBytes);
            payload = JsonSerializer.Deserialize<LicensePayload>(json);
        }
        catch
        {
            throw new InvalidOperationException("خطا در خواندن JSON payload.");
        }

        if (payload == null)
            throw new InvalidOperationException("payload خالی یا نامعتبر است.");

        // بررسی DeviceId
        var currentDeviceId = DeviceHelper.GetDeviceId();
        if (!string.Equals(payload.DeviceId, currentDeviceId, StringComparison.Ordinal))
            throw new InvalidOperationException($"DeviceId تطابق ندارد. دستگاه شما: {currentDeviceId}, payload: {payload.DeviceId}");

        // چک تاریخ‌ها
        var startDate = new DateTime(payload.StartTicks);
        var endDate = new DateTime(payload.EndTicks);
        if (endDate <= DateTime.Now)
            throw new InvalidOperationException("لایسنس منقضی شده است.");

        // 📌 ذخیره وضعیت لایسنس در Preferences (مهم برای UI)
        var subscriptionType = payload.SubscriptionType ?? "Trial";
        Preferences.Set("IsSubscriptionActive", true);
        Preferences.Set("SubscriptionType", subscriptionType);
        Preferences.Set("LicenseStart", startDate.Ticks);
        Preferences.Set("LicenseEnd", endDate.Ticks);

        // برای UI: پلن و تاریخ پایان را هم ذخیره کنیم
        Preferences.Set("SubscriptionPlan", subscriptionType);
        Preferences.Set("SubscriptionEndDate", endDate.ToString("o")); // Roundtrip

        // ذخیره در DB
        using var conn = _databaseService.GetConnection();
        await conn.OpenAsync();
        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Licenses WHERE LicenseKey = $licenseKey";
        checkCmd.Parameters.AddWithValue("$licenseKey", signedLicense);
        long exists = (long)await checkCmd.ExecuteScalarAsync();
        if (exists == 0)
        {
            var license = new LicenseInfo
            {
                DeviceId = currentDeviceId,
                LicenseKey = signedLicense,
                UserPhone = Preferences.Get("UserPhone", string.Empty),
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                SubscriptionType = subscriptionType
            };
            await AddLicenseAsync(license);
        }

        return true;
    }

    // ✅ ایجاد یا گرفتن Trial فعال
    public async Task<LicenseInfo> CreateOrGetTrialAsync(string deviceId, string userPhone)
    {
        var existing = await GetActiveLicenseAsync(deviceId);
        if (existing != null && existing.SubscriptionType == "Trial")
        {
            if (existing.EndDate > DateTime.Now) return existing;
            Preferences.Set("IsSubscriptionActive", false);
        }

        var now = DateTime.Now;
        var trialEnd = now.AddDays(14); // نسخه اصلی: ۱۴ روزه

        var trialLicense = new LicenseInfo
        {
            DeviceId = deviceId,
            LicenseKey = Guid.NewGuid().ToString("N"),
            UserPhone = userPhone,
            StartDate = now,
            EndDate = trialEnd,
            IsActive = true,
            SubscriptionType = "Trial"
        };

        await AddLicenseAsync(trialLicense);

        // 📌 ذخیره اطلاعات Trial در Preferences
        Preferences.Set("IsSubscriptionActive", true);
        Preferences.Set("SubscriptionType", "Trial");
        Preferences.Set("LicenseStart", now.Ticks);
        Preferences.Set("LicenseEnd", trialEnd.Ticks);
        Preferences.Set("SubscriptionPlan", "Trial");
        Preferences.Set("SubscriptionEndDate", trialEnd.ToString("o"));

        return trialLicense;
    }

    // ✅ درج رکورد در DB
    public async Task AddLicenseAsync(LicenseInfo license)
    {
        using var conn = _databaseService.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Licenses (DeviceId, LicenseKey, UserPhone, StartDate, EndDate, IsActive, SubscriptionType)
            VALUES ($deviceId, $licenseKey, $userPhone, $startDate, $endDate, $isActive, $subscriptionType)";
        cmd.Parameters.AddWithValue("$deviceId", license.DeviceId);
        cmd.Parameters.AddWithValue("$licenseKey", license.LicenseKey ?? "");
        cmd.Parameters.AddWithValue("$userPhone", license.UserPhone ?? "");
        cmd.Parameters.AddWithValue("$startDate", license.StartDate.Ticks);
        cmd.Parameters.AddWithValue("$endDate", license.EndDate.Ticks);
        cmd.Parameters.AddWithValue("$isActive", license.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("$subscriptionType", license.SubscriptionType ?? "Trial");

        await cmd.ExecuteNonQueryAsync();
    }

    // ✅ گرفتن لایسنس فعال
    public async Task<LicenseInfo?> GetActiveLicenseAsync(string deviceId)
    {
        using var conn = _databaseService.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM Licenses
            WHERE DeviceId = $deviceId AND IsActive = 1
            ORDER BY EndDate DESC
            LIMIT 1";
        cmd.Parameters.AddWithValue("$deviceId", deviceId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new LicenseInfo
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                DeviceId = reader.GetString(reader.GetOrdinal("DeviceId")),
                LicenseKey = reader.IsDBNull(reader.GetOrdinal("LicenseKey")) ? string.Empty : reader.GetString(reader.GetOrdinal("LicenseKey")),
                UserPhone = reader.GetString(reader.GetOrdinal("UserPhone")),
                StartDate = new DateTime(reader.GetInt64(reader.GetOrdinal("StartDate"))),
                EndDate = new DateTime(reader.GetInt64(reader.GetOrdinal("EndDate"))),
                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
                SubscriptionType = reader.GetString(reader.GetOrdinal("SubscriptionType"))
            };
        }
        return null;
    }

    public async Task<bool> IsLicenseValidAsync(string deviceId)
    {
        var license = await GetActiveLicenseAsync(deviceId);
        var isValid = license != null && license.IsActive && DateTime.Now <= license.EndDate;

        Preferences.Set("IsSubscriptionActive", isValid);
        if (license != null)
        {
            Preferences.Set("SubscriptionPlan", license.SubscriptionType);
            Preferences.Set("SubscriptionEndDate", license.EndDate.ToString("o"));
        }

        return isValid;
    }

    public bool CanUseLawyerFeatures()
    {
        bool isLawyer = Preferences.Get("IsLawyerRegistered", false);
        bool isActive = Preferences.Get("IsSubscriptionActive", false);
        return isLawyer && isActive;
    }

    public (DateTime start, DateTime end, string type) GetCurrentLicenseInfo()
    {
        long startTicks = Preferences.Get("LicenseStart", 0L);
        long endTicks = Preferences.Get("LicenseEnd", 0L);
        string type = Preferences.Get("SubscriptionType", "Trial");

        return (new DateTime(startTicks), new DateTime(endTicks), type);
    }
}
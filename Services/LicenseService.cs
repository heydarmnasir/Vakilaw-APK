using Microsoft.Data.Sqlite;
using Vakilaw.Models;

namespace Vakilaw.Services;

public class LicenseService
{
    private readonly DatabaseService _databaseService;

    public LicenseService(DatabaseService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
    }

    /// <summary>
    /// افزودن اشتراک یا Trial جدید
    /// </summary>
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

    /// <summary>
    /// ایجاد Trial جدید ۱۴ روزه برای کاربر وکیل، در صورتی که قبلاً Trial فعال وجود نداشته باشد
    /// </summary>
    public async Task<LicenseInfo> CreateTrialAsync(string deviceId, string userPhone)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentNullException(nameof(deviceId));

        if (string.IsNullOrWhiteSpace(userPhone))
            throw new ArgumentNullException(nameof(userPhone));

        // بررسی وجود Trial فعال قبلی
        var existing = await GetActiveLicenseAsync(deviceId);
        if (existing != null && existing.SubscriptionType == "Trial" && existing.EndDate > DateTime.Now)
        {
            return existing; // اگر Trial فعال وجود داشت، همان را برگردان
        }

        var now = DateTime.Now;
        var trialEnd = now.AddDays(14);

        var trialLicense = new LicenseInfo
        {
            DeviceId = deviceId,
            LicenseKey = Guid.NewGuid().ToString("N"), // کلید یکتا
            UserPhone = userPhone,
            StartDate = now,
            EndDate = trialEnd,
            IsActive = true,
            SubscriptionType = "Trial"
        };

        await AddLicenseAsync(trialLicense);
        return trialLicense;
    }

    /// <summary>
    /// فعال سازی لایسنس وارد شده توسط کاربر
    /// </summary>
    public async Task<bool> ActivateLicenseAsync(string deviceId, string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return false;

        using var conn = _databaseService.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*) FROM Licenses
            WHERE LicenseKey = $licenseKey AND DeviceId IS NULL AND IsActive = 0 LIMIT 1";
        cmd.Parameters.AddWithValue("$licenseKey", licenseKey);

        long count = (long)await cmd.ExecuteScalarAsync();
        if (count == 0)
            return false; // لایسنس معتبر نیست یا قبلاً استفاده شده

        // اختصاص لایسنس به دستگاه
        var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = @"
            UPDATE Licenses
            SET DeviceId = $deviceId, IsActive = 1
            WHERE LicenseKey = $licenseKey";
        updateCmd.Parameters.AddWithValue("$deviceId", deviceId);
        updateCmd.Parameters.AddWithValue("$licenseKey", licenseKey);

        await updateCmd.ExecuteNonQueryAsync();
        return true;
    }

    /// <summary>
    /// بررسی وضعیت Trial یا اشتراک فعلی
    /// </summary>
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
                LicenseKey = reader.IsDBNull(reader.GetOrdinal("LicenseKey"))
             ? string.Empty
             : reader.GetString(reader.GetOrdinal("LicenseKey")),
                UserPhone = reader.GetString(reader.GetOrdinal("UserPhone")),
                StartDate = new DateTime(reader.GetInt64(reader.GetOrdinal("StartDate"))),
                EndDate = new DateTime(reader.GetInt64(reader.GetOrdinal("EndDate"))),
                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
                SubscriptionType = reader.GetString(reader.GetOrdinal("SubscriptionType"))
            };
        }
        return null;
    }

    /// <summary>
    /// بررسی اعتبار اشتراک یا Trial
    /// </summary>
    public async Task<bool> IsLicenseValidAsync(string deviceId)
    {
        var license = await GetActiveLicenseAsync(deviceId);
        if (license == null)
            return false;

        return license.IsActive && DateTime.Now <= license.EndDate;
    }
}
using Vakilaw.Models;

namespace Vakilaw.Services;

public class UserService
{
    private readonly DatabaseService _db;

    public UserService(DatabaseService db)
    {
        _db = db;
    }

    /// <summary>
    /// ثبت‌نام کاربر جدید
    /// </summary>
    /// <param name="fullName">نام کامل</param>
    /// <param name="phone">شماره موبایل</param>
    /// <param name="role">نقش: "Lawyer" یا "Client"</param>
    /// <param name="licenseNumber">شماره پروانه وکالت (فقط برای وکیل)</param>
    /// <returns>User ثبت‌شده</returns>
    /// <exception cref="Exception">اگر شماره موبایل تکراری باشد</exception>
    public async Task<User> RegisterUserAsync(string fullName, string phone, string role, string? licenseNumber)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        // بررسی اینکه شماره موبایل قبلاً ثبت نشده باشد
        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE PhoneNumber = $phone";
        checkCmd.Parameters.AddWithValue("$phone", phone);

        var count = (long)await checkCmd.ExecuteScalarAsync();
        if (count > 0)
            throw new Exception("این شماره موبایل قبلاً ثبت‌نام شده است.");

        // ثبت کاربر جدید
        var insertCmd = conn.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO Users (FullName, PhoneNumber, Role, LicenseNumber)
            VALUES ($name, $phone, $role, $license);
            SELECT last_insert_rowid();";

        insertCmd.Parameters.AddWithValue("$name", fullName);
        insertCmd.Parameters.AddWithValue("$phone", phone);
        insertCmd.Parameters.AddWithValue("$role", role);
        insertCmd.Parameters.AddWithValue("$license", licenseNumber ?? "");

        var id = (long)await insertCmd.ExecuteScalarAsync();

        return new User
        {
            Id = (int)id,
            FullName = fullName,
            PhoneNumber = phone,
            Role = role,
            LicenseNumber = licenseNumber
        };
    }
}
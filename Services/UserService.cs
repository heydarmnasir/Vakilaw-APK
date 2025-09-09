using Vakilaw.Models;
using Vakilaw.Services;

namespace Vakilaw.Services;
public class UserService
{
    private readonly DatabaseService _db;

    public UserService(DatabaseService db) => _db = db;

    public async Task<User> RegisterUserAsync(string fullName, string phone, string role, string? licenseNumber)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE PhoneNumber = $phone";
        checkCmd.Parameters.AddWithValue("$phone", phone);

        var count = (long)await checkCmd.ExecuteScalarAsync();
        if (count > 0) throw new Exception("این شماره موبایل قبلاً ثبت‌نام شده است.");

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

    public async Task<User?> GetUserByPhoneAsync(string phone)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FullName, PhoneNumber, Role, LicenseNumber FROM Users WHERE PhoneNumber = $phone";
        cmd.Parameters.AddWithValue("$phone", phone);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                PhoneNumber = reader.GetString(2),
                Role = reader.GetString(3),
                LicenseNumber = reader.GetString(4)
            };
        }

        return null;
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Users
            SET FullName = $name, Role = $role, LicenseNumber = $license
            WHERE PhoneNumber = $phone";
        cmd.Parameters.AddWithValue("$name", user.FullName);
        cmd.Parameters.AddWithValue("$role", user.Role);
        cmd.Parameters.AddWithValue("$license", user.LicenseNumber ?? "");
        cmd.Parameters.AddWithValue("$phone", user.PhoneNumber);

        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> DeleteUserAsync(User user)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", user.Id);

        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var list = new List<User>();
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FullName, PhoneNumber, Role, LicenseNumber FROM Users";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new User
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                PhoneNumber = reader.GetString(2),
                Role = reader.GetString(3),
                LicenseNumber = reader.GetString(4)
            });
        }

        return list;
    }
}
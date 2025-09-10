using Microsoft.Data.Sqlite;
using System.Text.Json;
using Vakilaw.Models;

namespace Vakilaw.Services;

public class LawyerService
{
    private readonly DatabaseService _databaseService;

    public LawyerService(DatabaseService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
    }

    public async Task SeedDataFromJsonAsync(string jsonPath)
    {
        if (!File.Exists(jsonPath)) return;

        var json = await File.ReadAllTextAsync(jsonPath);
        var lawyers = JsonSerializer.Deserialize<List<Lawyer>>(json) ?? new();

        using var conn = _databaseService.GetConnection();
        await conn.OpenAsync();

        using var transaction = conn.BeginTransaction();

        foreach (var lawyer in lawyers)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
                INSERT OR IGNORE INTO Lawyers (Id, FullName, City, PhoneNumber, Address)
                VALUES ($id, $fullName, $city, $phoneNumber, $address);";
            cmd.Parameters.AddWithValue("$id", lawyer.Id);
            cmd.Parameters.AddWithValue("$fullName", lawyer.FullName);
            cmd.Parameters.AddWithValue("$city", lawyer.City ?? "");
            cmd.Parameters.AddWithValue("$phoneNumber", lawyer.PhoneNumber ?? "");
            cmd.Parameters.AddWithValue("$address", lawyer.Address ?? "");
            await cmd.ExecuteNonQueryAsync();
        }

        transaction.Commit();
    }

    public async Task<List<Lawyer>> GetAllLawyersAsync()
    {
        var list = new List<Lawyer>();

        using var conn = _databaseService.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FullName, City, PhoneNumber, Address FROM Lawyers ORDER BY FullName;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Lawyer
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                City = reader.IsDBNull(2) ? null : reader.GetString(2),
                PhoneNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                Address = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return list;
    }

    public async Task<Lawyer?> GetLawyerByIdAsync(int id)
    {
        using var conn = _databaseService.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FullName, City, PhoneNumber, Address FROM Lawyers WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Lawyer
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                City = reader.IsDBNull(2) ? null : reader.GetString(2),
                PhoneNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                Address = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }
        return null;
    }
}
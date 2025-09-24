using Microsoft.Data.Sqlite;
using Vakilaw.Models;

namespace Vakilaw.Services;

public class TransactionService
{
    private readonly DatabaseService _db;

    public TransactionService(DatabaseService db)
    {
        _db = db;
    }

    // 📌 دریافت همه تراکنش‌ها
    public async Task<List<Transaction>> GetAll()
    {
        var list = new List<Transaction>();

        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Title, Amount, IsIncome, Date, Description FROM Transactions ORDER BY Date DESC";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Transaction
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Amount = reader.GetDouble(2),
                IsIncome = reader.GetInt32(3) == 1,
                Date = DateTime.Parse(reader.GetString(4)),
                Description = reader.IsDBNull(5) ? "" : reader.GetString(5)
            });
        }

        return list;
    }

    // 📌 افزودن تراکنش
    public async Task Add(Transaction transaction)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Transactions (Title, Amount, IsIncome, Date, Description)
            VALUES ($title, $amount, $isIncome, $date, $desc)";

        cmd.Parameters.AddWithValue("$title", transaction.Title);
        cmd.Parameters.AddWithValue("$amount", transaction.Amount);
        cmd.Parameters.AddWithValue("$isIncome", transaction.IsIncome ? 1 : 0);
        cmd.Parameters.AddWithValue("$date", transaction.Date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$desc", transaction.Description);

        await cmd.ExecuteNonQueryAsync();
    }

    // 📌 حذف تراکنش
    public async Task Delete(int id)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Transactions WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    // 📌 ویرایش تراکنش
    public async Task Update(Transaction transaction)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Transactions
            SET Title = $title,
                Amount = $amount,
                IsIncome = $isIncome,
                Date = $date,
                Description = $desc
            WHERE Id = $id";

        cmd.Parameters.AddWithValue("$title", transaction.Title);
        cmd.Parameters.AddWithValue("$amount", transaction.Amount);
        cmd.Parameters.AddWithValue("$isIncome", transaction.IsIncome ? 1 : 0);
        cmd.Parameters.AddWithValue("$date", transaction.Date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$desc", transaction.Description);
        cmd.Parameters.AddWithValue("$id", transaction.Id);

        await cmd.ExecuteNonQueryAsync();
    }
}
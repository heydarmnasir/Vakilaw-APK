using Microsoft.Data.Sqlite;
using Vakilaw.Models;
using Vakilaw.Services;

public class CaseService
{
    private readonly DatabaseService _dbService;

    public CaseService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task AddCase(Case caseItem)
    {
        using var connection = _dbService.GetConnection();
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Cases 
                (Title, CaseNumber, CourtName, JudgeName, StartDate, EndDate, Status, Description, ClientId)
            VALUES 
                ($title, $caseNumber, $courtName, $judgeName, $startDate, $endDate, $status, $description, $clientId);";

        cmd.Parameters.AddWithValue("$title", caseItem.Title);
        cmd.Parameters.AddWithValue("$caseNumber", caseItem.CaseNumber ?? "");
        cmd.Parameters.AddWithValue("$courtName", caseItem.CourtName ?? "");
        cmd.Parameters.AddWithValue("$judgeName", caseItem.JudgeName ?? "");
        cmd.Parameters.AddWithValue("$startDate", caseItem.StartDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$endDate", caseItem.EndDate?.ToString("yyyy-MM-dd") ?? "");
        cmd.Parameters.AddWithValue("$status", caseItem.Status ?? "");
        cmd.Parameters.AddWithValue("$description", caseItem.Description ?? "");
        cmd.Parameters.AddWithValue("$clientId", caseItem.ClientId);

        cmd.ExecuteNonQuery();
    }

    public List<Case> GetCasesByClient(int clientId)
    {
        var cases = new List<Case>();

        using var connection = _dbService.GetConnection();
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Cases WHERE ClientId = $clientId";
        cmd.Parameters.AddWithValue("$clientId", clientId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            cases.Add(new Case
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                CaseNumber = reader.GetString(2),
                CourtName = reader.GetString(3),
                JudgeName = reader.GetString(4),
                StartDate = DateTime.Parse(reader.GetString(5)),
                EndDate = string.IsNullOrEmpty(reader.GetString(6)) ? null : DateTime.Parse(reader.GetString(6)),
                Status = reader.GetString(7),
                Description = reader.GetString(8),
                ClientId = reader.GetInt32(9)
            });
        }

        return cases;
    }

    public Case GetCaseById(int caseId)
    {
        using var connection = _dbService.GetConnection();
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Cases WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", caseId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Case
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                CaseNumber = reader.GetString(2),
                CourtName = reader.GetString(3),
                JudgeName = reader.GetString(4),
                StartDate = DateTime.Parse(reader.GetString(5)),
                EndDate = string.IsNullOrEmpty(reader.GetString(6)) ? null : DateTime.Parse(reader.GetString(6)),
                Status = reader.GetString(7),
                Description = reader.GetString(8),
                ClientId = reader.GetInt32(9)
            };
        }

        return null;
    }

    public async Task UpdateCase(Case caseItem)
    {
        using var connection = _dbService.GetConnection();
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE Cases
            SET Title=$title,
                CaseNumber=$caseNumber,
                CourtName=$courtName,
                JudgeName=$judgeName,
                StartDate=$startDate,
                EndDate=$endDate,
                Status=$status,
                Description=$description,
                ClientId=$clientId
            WHERE Id=$id;";

        cmd.Parameters.AddWithValue("$title", caseItem.Title);
        cmd.Parameters.AddWithValue("$caseNumber", caseItem.CaseNumber ?? "");
        cmd.Parameters.AddWithValue("$courtName", caseItem.CourtName ?? "");
        cmd.Parameters.AddWithValue("$judgeName", caseItem.JudgeName ?? "");
        cmd.Parameters.AddWithValue("$startDate", caseItem.StartDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$endDate", caseItem.EndDate?.ToString("yyyy-MM-dd") ?? "");
        cmd.Parameters.AddWithValue("$status", caseItem.Status ?? "");
        cmd.Parameters.AddWithValue("$description", caseItem.Description ?? "");
        cmd.Parameters.AddWithValue("$clientId", caseItem.ClientId);
        cmd.Parameters.AddWithValue("$id", caseItem.Id);

        cmd.ExecuteNonQuery();
    }

    public async Task DeleteCase(int caseId)
    {
        using var connection = _dbService.GetConnection();
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Cases WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", caseId);

        cmd.ExecuteNonQuery();
    }

    public List<Case> SearchCases(string keyword)
    {
        var cases = new List<Case>();

        using var connection = _dbService.GetConnection();
        connection.Open();

        var cmd = connection.CreateCommand();

        if (string.IsNullOrWhiteSpace(keyword))
        {
            cmd.CommandText = @"
            SELECT c.Id, c.Title, c.CaseNumber, c.CourtName, c.JudgeName,
                   c.StartDate, c.EndDate, c.Status, c.Description,
                   c.ClientId, cl.FullName
            FROM Cases c
            JOIN Clients cl ON c.ClientId = cl.Id";
        }
        else
        {
            cmd.CommandText = @"
            SELECT c.Id, c.Title, c.CaseNumber, c.CourtName, c.JudgeName,
                   c.StartDate, c.EndDate, c.Status, c.Description,
                   c.ClientId, cl.FullName
            FROM Cases c
            JOIN Clients cl ON c.ClientId = cl.Id
            WHERE c.Title LIKE $kw
               OR c.CaseNumber LIKE $kw
               OR cl.FullName LIKE $kw";
            cmd.Parameters.AddWithValue("$kw", "%" + keyword.Trim() + "%");
        }

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            cases.Add(new Case
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                CaseNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
                CourtName = reader.IsDBNull(3) ? null : reader.GetString(3),
                JudgeName = reader.IsDBNull(4) ? null : reader.GetString(4),
                StartDate = reader.IsDBNull(5) ? DateTime.MinValue : DateTime.Parse(reader.GetString(5)),
                EndDate = reader.IsDBNull(6) ? DateTime.MinValue : DateTime.Parse(reader.GetString(6)),
                Status = reader.IsDBNull(7) ? null : reader.GetString(7),
                Description = reader.IsDBNull(8) ? null : reader.GetString(8),
                ClientId = reader.GetInt32(9),

                // توجه: اگه بخوای نام موکل رو مستقیم داخل مدل داشته باشی می‌تونی پراپرتی اضافه کنی
                Client = new Client
                {
                    Id = reader.GetInt32(9),
                    FullName = reader.GetString(10)
                }
            });
        }

        return cases;
    }
}
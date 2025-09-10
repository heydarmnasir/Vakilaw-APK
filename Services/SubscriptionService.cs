using Microsoft.Data.Sqlite;

namespace Vakilaw.Services
{  
    public class SubscriptionService
    {
        private readonly string _dbPath;

        public SubscriptionService(string dbPath)
        {
            _dbPath = dbPath;
        }

        // شروع Trial
        public void StartTrial(int userId)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            // اگر قبلاً Trial داشته
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Subscriptions WHERE UserId=@uid AND IsTrial=1";
            checkCmd.Parameters.AddWithValue("@uid", userId);

            var exists = Convert.ToInt32(checkCmd.ExecuteScalar());
            if (exists > 0) return;

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
            INSERT INTO Subscriptions (UserId, StartDate, EndDate, Type, IsTrial)
            VALUES (@uid, @start, @end, @type, 1)";
            insertCmd.Parameters.AddWithValue("@uid", userId);
            insertCmd.Parameters.AddWithValue("@start", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            insertCmd.Parameters.AddWithValue("@end", DateTime.Now.AddDays(14).ToString("yyyy-MM-dd HH:mm:ss"));
            insertCmd.Parameters.AddWithValue("@type", "Trial");
            insertCmd.ExecuteNonQuery();
        }

        // بررسی اعتبار اشتراک
        public bool IsSubscriptionValid(int userId)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            SELECT COUNT(*) FROM Subscriptions
            WHERE UserId=@uid AND EndDate >= @now";
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        // خرید یا تمدید اشتراک
        public void AddOrRenewSubscription(int userId, string type, string trackingCode)
        {
            int months = type switch
            {
                "3Month" => 3,
                "6Month" => 6,
                "Yearly" => 12,
                _ => 0
            };
            if (months == 0) throw new ArgumentException("نوع اشتراک نامعتبر است");

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            // آخرین اشتراک فعال
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = @"
            SELECT EndDate FROM Subscriptions
            WHERE UserId=@uid AND EndDate >= @now
            ORDER BY EndDate DESC LIMIT 1";
            checkCmd.Parameters.AddWithValue("@uid", userId);
            checkCmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var result = checkCmd.ExecuteScalar();
            DateTime startDate = DateTime.Now;
            if (result != null && DateTime.TryParse(result.ToString(), out var lastEnd))
                startDate = lastEnd.AddDays(1);

            DateTime endDate = startDate.AddMonths(months);

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
            INSERT INTO Subscriptions (UserId, StartDate, EndDate, Type, IsTrial, PaymentTrackingCode)
            VALUES (@uid, @start, @end, @type, 0, @tracking)";
            insertCmd.Parameters.AddWithValue("@uid", userId);
            insertCmd.Parameters.AddWithValue("@start", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
            insertCmd.Parameters.AddWithValue("@end", endDate.ToString("yyyy-MM-dd HH:mm:ss"));
            insertCmd.Parameters.AddWithValue("@type", type);
            insertCmd.Parameters.AddWithValue("@tracking", trackingCode ?? "");
            insertCmd.ExecuteNonQuery();
        }
    }
}
using System.Net.Http.Json;
using System.Text.Json;
using Vakilaw.Models;
using Vakilaw.Services;

public interface ISmsService
{
    Task<string> SendOtpAsync(string phoneNumber);
    Task<bool> SendSingleAsync(string to, string text, string clientName = "");
    Task<Dictionary<string, bool>> SendGroupAsync(List<string> recipients, string text);
}

public class SmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseService _db;
    // ⚠️ مقادیر از پنل ملی پیامک

    private readonly string _from = "50002710049043"; // شماره خط اختصاصی
    private readonly string _otpApiKey = "ae33a0fc4afc4988b1ecd8d62253dd9b"; // کلید OTP

    public SmsService(DatabaseService db)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://console.melipayamak.com/")
        };
        _db = db;
    }

    // ===============================
    // ارسال OTP (از ملی پیامک)
    // ===============================
    public async Task<string> SendOtpAsync(string phoneNumber)
    {
        var payload = new { to = phoneNumber };

        var response = await _httpClient.PostAsJsonAsync($"api/send/otp/{_otpApiKey}", payload);
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            throw new Exception($"خطا در اتصال به ملی پیامک: {errorText}");
        }

        var result = await response.Content.ReadFromJsonAsync<OtpResponse>();
        if (result == null || string.IsNullOrWhiteSpace(result.code))
            throw new Exception("پاسخ نامعتبر از ملی پیامک");

        // ✅ حذف شرطی که باعث خطای نادرست می‌شد
        // حالا صرفاً وقتی status واقعا خطا است Exception پر می‌کنیم
        if (!string.IsNullOrWhiteSpace(result.status) &&
            (result.status.Contains("خطا") || result.status.Contains("error", StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"خطا در ارسال کد: {result.status}");
        }

        return result.code; // کد واقعی OTP برای ViewModel
    }

    // ===============================
    // پیامک تک‌گیرنده
    // ===============================
    public async Task<bool> SendSingleAsync(string to, string text, string clientName = "")
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri("https://console.melipayamak.com") };

            var payload = new
            {
                from = _from,
                to = to,   // string تنها برای تک گیرنده
                text = $"{text}\nلغو11"
            };

            // لاگ payload
            var payloadJson = JsonSerializer.Serialize(payload);
            System.Diagnostics.Debug.WriteLine($"[SendSingleAsync] Payload: {payloadJson}");

            var result = await client.PostAsJsonAsync($"api/send/simple/{_otpApiKey}", payload);
            var response = await result.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[SendSingleAsync] StatusCode: {result.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[SendSingleAsync] Response Raw: {response}");

            if (!result.IsSuccessStatusCode)
                throw new Exception($"StatusCode: {result.StatusCode}, Response: {response}");

            // 1) تلاش اول با JsonSerializer (case-insensitive)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            MeliResponse? json = null;
            try
            {
                json = JsonSerializer.Deserialize<MeliResponse>(response, options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SendSingleAsync] Deserialize with JsonSerializer failed: " + ex);
                // نادیده می‌گیریم تا fallback انجام شود
            }

            long recId = 0;
            string statusText = string.Empty;

            if (json != null)
            {
                recId = json.RecId;
                statusText = json.Status ?? string.Empty;
                System.Diagnostics.Debug.WriteLine($"[SendSingleAsync] Deserialized: RecId={recId}, Status='{statusText}'");
            }
            else
            {
                // 2) fallback: parse با JsonDocument (برای زمانی که recId به صورت string است یا نام متفاوت دارد)
                try
                {
                    using var doc = JsonDocument.Parse(response);
                    var root = doc.RootElement;

                    // تلاش برای دریافت recId با چند نام متداول
                    if (root.TryGetProperty("recId", out var rid) ||
                        root.TryGetProperty("RecId", out rid) ||
                        root.TryGetProperty("recid", out rid))
                    {
                        if (rid.ValueKind == JsonValueKind.Number && rid.TryGetInt64(out var v))
                            recId = v;
                        else if (rid.ValueKind == JsonValueKind.String && long.TryParse(rid.GetString(), out var v2))
                            recId = v2;
                    }

                    if (root.TryGetProperty("status", out var st) || root.TryGetProperty("Status", out st))
                        statusText = st.GetString() ?? string.Empty;

                    System.Diagnostics.Debug.WriteLine($"[SendSingleAsync][fallback] RecId={recId}, Status='{statusText}'");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[SendSingleAsync] JsonDocument parse failed: " + ex);
                }
            }

            // 3) تصمیم‌گیری: اگر status حاوی کلمه خطا است -> خطا، 
            // وگرنه اگر recId>0 => موفق، در غیر این صورت خطا همراه با محتوای کامل پاسخ
            if (!string.IsNullOrEmpty(statusText) &&
                (statusText.Contains("خطا") || statusText.Contains("error", StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception($"پیامک ارسال نشد: {statusText}. RawResponse: {response}");
            }
        
            // تعیین نتیجه
            bool success = (recId > 0) && string.IsNullOrEmpty(statusText);

            // ✅ ذخیره تاریخچه
            await SaveHistory(new SmsHistoryItem
            {
                ClientName = clientName,
                PhoneNumber = to,
                Message = text,
                SetDate = DateTime.Now, // ✅ میلادی ذخیره میشه
                StatusText = success ? "ارسال شد" : (string.IsNullOrEmpty(statusText) ? "ناموفق" : statusText),
                IsGroup = false
            });

            return success;
        }
        catch (Exception ex)
        {
            // در صورت خطا هم تاریخچه ذخیره میشه
            await SaveHistory(new SmsHistoryItem
            {
                ClientName = clientName,
                PhoneNumber = to,
                Message = text,
                SetDate = DateTime.Now,
                StatusText = $"خطا: {ex.Message}",
                IsGroup = false
            });

            throw;        
        }
    }

    // ===============================
    // پیامک گروهی
    // ===============================
    public async Task<Dictionary<string, bool>> SendGroupAsync(List<string> recipients, string text)
    {
        var results = new Dictionary<string, bool>();

        foreach (var number in recipients)
        {
            try
            {
                // ClientName گروهی رو می‌تونی خالی بذاری یا از بیرون پاس بدی
                bool success = await SendSingleAsync(number, text, "ارسال گروهی");
                results[number] = success;
            }
            catch
            {
                results[number] = false;
            }
        }

        return results;
    }

    private class OtpResponse
    {
        public string code { get; set; } = "";
        public string status { get; set; } = "";
    }

    public class MeliResponse
    {
        // حتماً با long استفاده کنید چون recId ممکنه بزرگتر از int باشد
        public long RecId { get; set; }
        public string Status { get; set; }
    }

    private async Task SaveHistory(SmsHistoryItem item)
    {
        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO SmsHistory 
            (ClientName, PhoneNumber, Message, SetDate, StatusText, IsGroup) 
            VALUES ($ClientName, $PhoneNumber, $Message, $SetDate, $StatusText, $IsGroup)";
        cmd.Parameters.AddWithValue("$ClientName", item.ClientName ?? "");
        cmd.Parameters.AddWithValue("$PhoneNumber", item.PhoneNumber ?? "");
        cmd.Parameters.AddWithValue("$Message", item.Message ?? "");
        cmd.Parameters.AddWithValue("$SetDate", item.SetDate);
        cmd.Parameters.AddWithValue("$StatusText", item.StatusText);
        cmd.Parameters.AddWithValue("$IsGroup", item.IsGroup ? 1 : 0);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<SmsHistoryItem>> GetHistoryAsync()
    {
        var list = new List<SmsHistoryItem>();

        using var conn = _db.GetConnection();
        await conn.OpenAsync();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, ClientName, PhoneNumber, Message, SetDate, StatusText, IsGroup FROM SmsHistory ORDER BY Id DESC";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new SmsHistoryItem
            {
                Id = reader.GetInt32(0),
                ClientName = reader.GetString(1),
                PhoneNumber = reader.GetString(2),
                Message = reader.GetString(3),
                SetDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                StatusText = reader.GetString(5),
                IsGroup = reader.GetInt32(6) == 1
            });
        }
        return list;
    }
}
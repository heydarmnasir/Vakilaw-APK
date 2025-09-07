using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vakilaw.Converters
{
    public class StringOrArrayToListConverter : JsonConverter<List<string>>
    {
        public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // اگر رشته بود → تبدیل به لیست با یک آیتم
            if (reader.TokenType == JsonTokenType.String)
            {
                return new List<string> { reader.GetString()! };
            }

            // اگر آرایه بود → همه‌ی رشته‌ها رو بخون
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var list = new List<string>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        list.Add(reader.GetString()!);
                    }
                }
                return list;
            }

            // در غیر این صورت مقدار خالی
            return new List<string>();
        }

        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
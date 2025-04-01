using System.Text.Json;
using System.Text.Json.Serialization;

namespace NavalBattle.Core.Helpers
{
    internal static class JsonHelper
    {
        private static JsonSerializerOptions GetJsonSerializerOptions()
            => new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

        public static string Serialize<T>(this T objectToSerialize) 
            => JsonSerializer.Serialize(objectToSerialize, GetJsonSerializerOptions());

        public static T Deserialize<T>(this string stringObject) where T : class
            => JsonSerializer.Deserialize<T>(stringObject, GetJsonSerializerOptions());
    }
} 
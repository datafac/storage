using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Text;

namespace DTOMaker.Runtime.JsonNewtonSoft
{
    public static class SerializationHelpers
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        private static readonly JsonSerializer _serializer = JsonSerializer.Create(_settings);

        public static string ToJson<T>(this T value)
        {
            using var sw = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
            using var jw = new JsonTextWriter(sw);
            jw.Formatting = _serializer.Formatting;
            _serializer.Serialize(jw, value, typeof(T));
            return sw.ToString();
        }

        public static T? FromJson<T>(this string input)
        {
            using var sr = new StringReader(input);
            using var jr = new JsonTextReader(sr);
            return _serializer.Deserialize<T>(jr);
        }
    }
}

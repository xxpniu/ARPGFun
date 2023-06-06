using Google.Protobuf;
using Proto;

namespace Core
{
    public static class Extends 
    {
        public static bool IsOk(this ErrorCode er)
        {
            return er == ErrorCode.Ok;
        }

        private static readonly JsonParser parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
        private static readonly JsonFormatter format = new JsonFormatter(JsonFormatter.Settings.Default.WithFormatEnumsAsIntegers(true).WithFormatDefaultValues(true));

        public static string ToJson<T>(this T msg) where T : IMessage, new()
        {
            return format.Format(msg);
        }

        public static T Parser<T>(this string json) where T : IMessage, new()
        {
            return parser.Parse<T>(json);
        }
    }

}
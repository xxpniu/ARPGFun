using System;
using Google.Protobuf;

namespace Utility
{
    public static class Extends
    {

        static JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));


        public static T TryParseMessage<T>(this string json) where T : IMessage, new()
        {
            return jsonParser.Parse<T>(json);
        }

        public static string ToJson(this IMessage msg)
        {
            return JsonFormatter.Default.Format(msg);
        }


    }
}

using System.Text.Json;
using MessagePack;

namespace AscNet.GameServer
{
    internal static class MessagePackPayloads
    {
        public static byte[] FromJson(string json)
        {
            return Serialize(PayloadFromJson(json));
        }

        public static byte[] Serialize(Dictionary<string, object?> payload)
        {
            return MessagePackSerializer.Serialize(payload);
        }

        public static Dictionary<string, object?> PayloadFromJson(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return (Dictionary<string, object?>)JsonElementToMessagePackValue(document.RootElement)!;
        }

        private static object? JsonElementToMessagePackValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(property => property.Name, property => JsonElementToMessagePackValue(property.Value)),
                JsonValueKind.Array => element.EnumerateArray()
                    .Select(JsonElementToMessagePackValue)
                    .ToArray(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number when element.TryGetInt64(out long integer) =>
                    integer is >= int.MinValue and <= int.MaxValue ? (int)integer : integer,
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => null
            };
        }
    }
}

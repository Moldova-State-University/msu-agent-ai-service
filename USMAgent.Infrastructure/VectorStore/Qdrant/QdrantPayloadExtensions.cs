using Qdrant.Client.Grpc;

namespace USMAgent.Infrastructure.VectorStore.Qdrant;

internal static class QdrantPayloadExtensions
{
    public static string GetString(this IDictionary<string, Value> payload, string key)
    {
        if (!payload.TryGetValue(key, out var value))
            return string.Empty;

        return value.KindCase switch
        {
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.IntegerValue => value.IntegerValue.ToString(),
            Value.KindOneofCase.DoubleValue => value.DoubleValue.ToString(),
            Value.KindOneofCase.BoolValue => value.BoolValue.ToString(),
            _ => value.ToString()
        };
    }
}
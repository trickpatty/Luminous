using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;

namespace Luminous.Infrastructure.Persistence;

/// <summary>
/// Custom CosmosDB serializer that uses System.Text.Json with proper support
/// for [JsonInclude] attributes on private setters.
/// </summary>
public sealed class SystemTextJsonCosmosSerializer : CosmosSerializer
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonCosmosSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

    public override T FromStream<T>(Stream stream)
    {
        if (stream == null || !stream.CanRead)
        {
            throw new ArgumentException("Stream is null or cannot be read", nameof(stream));
        }

        // Handle case where stream has already been read or is at end
        if (stream.CanSeek && stream.Length == 0)
        {
            return default!;
        }

        using var streamReader = new StreamReader(stream);
        var json = streamReader.ReadToEnd();

        if (string.IsNullOrEmpty(json))
        {
            return default!;
        }

        return JsonSerializer.Deserialize<T>(json, _options)!;
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, input, _options);
        stream.Position = 0;
        return stream;
    }
}

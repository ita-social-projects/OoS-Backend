using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OutOfSchool.Common.Extensions;

public static class StreamExtensions
{
    public static T ReadAndDeserializeFromJson<T>(this Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new NotSupportedException("Can't read this stream");
        }

        using var streamReader = new StreamReader(stream);
        byte[] jsonData = Encoding.UTF8.GetBytes(streamReader.ReadToEnd());

        // Create a Utf8JsonReader with the bytes
        var reader = new Utf8JsonReader(jsonData, isFinalBlock: true, state: default);

        // Deserialize JSON data using Utf8JsonReader
        return JsonSerializer.Deserialize<T>(ref reader);
    }

    public static void SerializeToJsonAndWrite<T>(this Stream stream, T objectToWrite)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanWrite)
        {
            throw new NotSupportedException("Can't write to this stream");
        }

        using var jsonTextWriter = new Utf8JsonWriter(stream);

        JsonSerializer.Serialize(jsonTextWriter, objectToWrite);
        jsonTextWriter.Flush();
    }
}
using System;
using System.IO;
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

        using (stream)
        {
            // Define buffer size
            Span<byte> buffer = new byte[1024];

            // Read data from the stream into the buffer
            int bytesRead = stream.Read(buffer);

            // Initialize the Utf8JsonReader with the buffer slice containing actual data
            var reader = new Utf8JsonReader(buffer[..bytesRead], isFinalBlock: true, state: default);

            // Deserialize JSON data using Utf8JsonReader
            return JsonSerializer.Deserialize<T>(ref reader);
        }
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
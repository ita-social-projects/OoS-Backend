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

        return JsonSerializerHelper.Deserialize<T>(stream);
    }

    public static void SerializeToJsonAndWrite<T>(this Stream stream, T objectToWrite)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanWrite)
        {
            throw new NotSupportedException("Can't write to this stream");
        }

        using var jsonTextWriter = new Utf8JsonWriter(stream);

        JsonSerializerHelper.Serialize(jsonTextWriter, objectToWrite);
        jsonTextWriter.Flush();
    }
}
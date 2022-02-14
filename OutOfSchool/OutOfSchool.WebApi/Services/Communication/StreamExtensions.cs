﻿using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace OutOfSchool.WebApi.Services.Communication
{
    public static class StreamExtensions
    {
        public static T ReadAndDeserializeFromJson<T>(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new NotSupportedException("Can't read this stream");
            }

            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var jsonSerializer = new JsonSerializer();

                    return jsonSerializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public static void SerializeToJsonAndWrite<T>(this Stream stream, T objectToWrite)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new NotSupportedException("Can't write to this stream");
            }

            using (var streamWriter = new StreamWriter(stream, new UTF8Encoding(), CommunicationConstants.BufferSize, true))
            {
                using (var jsonTextWriter = new JsonTextWriter(streamWriter))
                {
                    var jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(jsonTextWriter, objectToWrite);
                    jsonTextWriter.Flush();
                }
            }
        }
    }
}

using System;
using System.IO;
using System.Text.Json;

#nullable enable

namespace OutOfSchool.Common;

public static class JsonSerializerHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptionsWeb = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Parses the text representing a single JSON value into a <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON value into.</typeparam>
    /// <returns>A <typeparamref name="TValue"/> representation of the JSON value.</returns>
    /// <param name="json">JSON text to parse.</param>
    /// <param name="options">Options to control the behavior during parsing.</param>
    public static TValue? Deserialize<TValue>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<TValue>(json, options ?? JsonSerializerOptionsWeb);
    }

    /// <summary>
    /// Reads the UTF-8 encoded text representing a single JSON value into a <typeparamref name="TValue"/>.
    /// The Stream will be read to completion.
    /// </summary>
    /// <typeparam name="TValue">The type to deserialize the JSON value into.</typeparam>
    /// <returns>A <typeparamref name="TValue"/> representation of the JSON value.</returns>
    /// <param name="stream">JSON data to parse.</param>
    /// <param name="options">Options to control the behavior during reading.</param>
    /// <exception cref="System.ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    public static TValue? Deserialize<TValue>(Stream stream, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        return JsonSerializer.Deserialize<TValue>(stream, options ?? JsonSerializerOptionsWeb);
    }

    /// <summary>
    /// Parses the text representing a single JSON value into a <paramref name="returnType"/>.
    /// </summary>
    /// <returns>A <paramref name="returnType"/> representation of the JSON value.</returns>
    /// <param name="json">JSON text to parse.</param>
    /// <param name="returnType">The type of the object to convert to and return.</param>
    /// <param name="options">Options to control the behavior during parsing.</param>
    public static object? Deserialize(string json, Type returnType, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize(json, returnType, options ?? JsonSerializerOptionsWeb);
    }

    /// <summary>
    /// Converts the provided value into a <see cref="string"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
    /// <returns>A <see cref="string"/> representation of the value.</returns>
    /// <param name="value">The value to convert.</param>
    /// <param name="options">Options to control the conversion behavior.</param>
    public static string Serialize<TValue>(TValue value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options ?? JsonSerializerOptionsWeb);
    }

    /// <summary>
    /// Writes one JSON value (including objects or arrays) to the provided writer.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to serialize.</typeparam>
    /// <param name="writer">The writer to write.</param>
    /// <param name="value">The value to convert and write.</param>
    /// <param name="options">Options to control the behavior.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="writer"/> is <see langword="null"/>.
    /// </exception>
    public static void Serialize<TValue>(Utf8JsonWriter writer, TValue value, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);

        JsonSerializer.Serialize(writer, value, options ?? JsonSerializerOptionsWeb);
    }
}
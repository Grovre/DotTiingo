using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotTiingo.Model.Rest;

/// <summary>
/// Handles JSON numbers that are serialized as floats (e.g. 288688.0)
/// but need to be deserialized into a long.
/// </summary>
internal class LenientInt64Converter : JsonConverter<long>
{
    public override long Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (reader.TryGetInt64(out var v))
            return v;

        if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), out var sv))
            return sv;

        // The value has a fractional part (e.g. 288688.0);
        // read as double and cast.
        var dv = reader.GetDouble();
        return (long)dv;
    }

    public override void Write(
        Utf8JsonWriter writer,
        long value,
        JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
/// Handles JSON numbers that may be null or serialized as floats (e.g. 288688.0)
/// but need to be deserialized into a nullable long.
/// </summary>
internal class LenientNullableInt64Converter : JsonConverter<long?>
{
    public override long? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TryGetInt64(out var v))
            return v;

        if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), out var sv))
            return sv;

        var dv = reader.GetDouble();
        return (long)dv;
    }

    public override void Write(
        Utf8JsonWriter writer,
        long? value,
        JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteNumberValue(value.Value);
    }
}

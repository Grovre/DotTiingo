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

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out var v))
                return v;
            var dv = reader.GetDouble();
            return (long)dv;
        }

        if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), out var sv))
            return sv;

        return default;
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

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out var v))
                return v;
            var dv = reader.GetDouble();
            return (long)dv;
        }

        if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), out var sv))
            return sv;

        return null;
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

/// <summary>
/// Handles JSON numbers that may be null, serialized as floats/integers,
/// or non-numeric strings (e.g. "Field not available for free/evaluation")
/// and deserializes into a nullable int.
/// </summary>
internal class LenientNullableInt32Converter : JsonConverter<int?>
{
    public override int? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out var v))
                return v;
            var dv = reader.GetDouble();
            return (int)dv;
        }

        if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out var sv))
            return sv;

        return null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        int? value,
        JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteNumberValue(value.Value);
    }
}

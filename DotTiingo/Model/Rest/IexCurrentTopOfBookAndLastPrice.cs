using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DotTiingo.Model.Rest;

public record IexCurrentTopOfBookAndLastPrice(
    string Ticker,
    DateTimeOffset Timestamp,
    DateTimeOffset? QuoteTimestamp,
    DateTimeOffset? LastSaleTimestamp,
    float? Last,
    int? LastSize,
    float TngoLast,
    float PrevClose,
    float Open,
    float High,
    float Low,
    float? Mid,
    [property: JsonConverter(typeof(LenientInt64Converter))]
    long Volume,
    float? BidSize,
    float? BidPrice,
    float? AskSize,
    float? AskPrice);

/// <summary>
/// Handles JSON numbers that are serialized as floats (e.g. 288688.0)
/// but need to be deserialized into a long. Tiingo's IEX API documents
/// volume as int64 but the live response includes a fractional component.
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
        throw new NotSupportedException();
    }
}

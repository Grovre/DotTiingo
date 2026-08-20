using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DotTiingo.Model.Rest;

/// <summary>
/// Represents a current reference price and liquidity snapshot for an equity asset.
/// </summary>
[Experimental("TNGOBETA")]
public record EquityRealtimeSnapshot(
    string Ticker,
    DateTimeOffset Timestamp,
    float? TngoLast,
    float? LqRefPrice,
    float? PrevClose,
    float? Open,
    float? High,
    float? Low,
    [property: JsonConverter(typeof(LenientNullableInt64Converter))]
    long? Volume,
    float? LqSpread,
    float? LqBidPrice,
    [property: JsonConverter(typeof(LenientNullableInt64Converter))]
    long? LqBidSize,
    float? LqAskPrice,
    [property: JsonConverter(typeof(LenientNullableInt64Converter))]
    long? LqAskSize);

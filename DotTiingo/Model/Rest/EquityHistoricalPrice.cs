using System;
using System.Text.Json.Serialization;

namespace DotTiingo.Model.Rest;

/// <summary>
/// Represents historical intraday price and volume data for an equity asset.
/// </summary>
public record EquityHistoricalPrice(
    DateTimeOffset Date,
    float Open,
    float High,
    float Low,
    float Close,
    [property: JsonConverter(typeof(LenientNullableInt64Converter))]
    long? Volume);

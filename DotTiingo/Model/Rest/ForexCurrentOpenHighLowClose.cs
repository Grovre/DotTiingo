using System;
using System.Diagnostics.CodeAnalysis;

namespace DotTiingo.Model.Rest;

/// <summary>
/// Represents open, high, low, close (OHLC) price data for a forex pair.
/// </summary>
[Experimental("TNGOBETA")]
public record ForexCurrentOpenHighLowClose(
    DateTimeOffset Date,
    string Ticker,
    float Open,
    float High,
    float Low,
    float Close);

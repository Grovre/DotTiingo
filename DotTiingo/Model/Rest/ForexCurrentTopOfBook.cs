using System;
using System.Diagnostics.CodeAnalysis;

namespace DotTiingo.Model.Rest;

/// <summary>
/// Represents current top-of-book and quote data for a forex pair.
/// </summary>
[Experimental("TNGOBETA")]
public record ForexCurrentTopOfBook(
    string Ticker,
    DateTimeOffset QuoteTimestamp,
    float MidPrice,
    float BidSize,
    float BidPrice,
    float AskSize,
    float AskPrice);

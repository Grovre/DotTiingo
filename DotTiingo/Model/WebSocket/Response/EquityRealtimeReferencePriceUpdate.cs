using System;
using System.Diagnostics.CodeAnalysis;

namespace DotTiingo.Model.WebSocket.Response;

/// <summary>
/// Represents a consolidated equity reference price update received over WebSocket.
/// </summary>
[Experimental("TNGOBETA")]
public record EquityRealtimeReferencePriceUpdate(
    DateTimeOffset Date,
    string Ticker,
    float ReferencePrice) : IResponseData;

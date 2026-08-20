using System;
using System.Diagnostics.CodeAnalysis;

namespace DotTiingo.Model.WebSocket.Response;

/// <summary>
/// Represents a consolidated equity reference price update received over WebSocket.
/// </summary>
// TODO: Check Tiingo for beta status
[Experimental("TNGOBETA")]
public record EquityRealtimeReferencePriceUpdate(
    DateTimeOffset Date,
    string Ticker,
    float ReferencePrice) : IResponseData;

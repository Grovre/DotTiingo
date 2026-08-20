using System;
using System.Diagnostics.CodeAnalysis;

namespace DotTiingo.Model.WebSocket.Response;

/// <summary>
/// Represents a consolidated equity liquidity risk metric update received over WebSocket.
/// </summary>
[Experimental("TNGOBETA")]
public record EquityRealtimeLiquidityRiskMetricUpdate(
    DateTimeOffset Date,
    string Ticker,
    float LiquiditySpread,
    int LiquidityBidSize,
    float LiquidityBidPrice,
    float ReferencePrice,
    float LiquidityAskPrice,
    int LiquidityAskSize) : IResponseData;

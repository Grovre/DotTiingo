using System;
using System.Diagnostics.CodeAnalysis;

namespace DotTiingo.Model.WebSocket.Response;

/// <summary>
/// Represents a consolidated equity liquidity risk metric update received over WebSocket.
/// </summary>
// TODO: Check Tiingo for beta status
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

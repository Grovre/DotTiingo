using DotTiingo.Model.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotTiingo.Api.WebSocket;

/// <summary>
/// Provides access to the Equity Realtime WebSocket API.
/// </summary>
[Experimental("TNGOBETA")]
public interface ITiingoWebSocketEquityRealtimeApi
{
    /// <summary>
    /// Connects to the Tiingo Equity Realtime WebSocket API.
    /// </summary>
    /// <param name="thresholdLevel">The threshold level for the connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An <see cref="ITiingoWebSocketConnection"/> instance.</returns>
    Task<ITiingoWebSocketConnection> Connect(EquityRealtimeThresholdLevel thresholdLevel, CancellationToken cancellationToken);
}

/// <summary>
/// Implementation of <see cref="ITiingoWebSocketEquityRealtimeApi"/>.
/// </summary>
[Experimental("TNGOBETA")]
internal class WebSocketEquityRealtimeApi : ITiingoWebSocketEquityRealtimeApi
{
    private const string BaseUrl = $"{TiingoApiHelper.WebSocketBaseUrl}/equity/intraday";
    private readonly string _token;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketEquityRealtimeApi"/> class.
    /// </summary>
    /// <param name="token">The Tiingo API token.</param>
    public WebSocketEquityRealtimeApi(string token)
    {
        _token = token;
    }

    /// <inheritdoc/>
    public Task<ITiingoWebSocketConnection> Connect(EquityRealtimeThresholdLevel thresholdLevel, CancellationToken cancellationToken)
    {
        var wsAuth = new WebSocketAuthorization("subscribe", _token, (int)thresholdLevel);
        var connFactory = new WebSocketConnectionFactory(wsAuth);
        return connFactory.CreateConnectionAsync(BaseUrl, cancellationToken);
    }
}

/// <summary>
/// Specifies the threshold level for the Tiingo Equity Realtime WebSocket feed, determining the type of data received.
/// </summary>
[Experimental("TNGOBETA")]
public enum EquityRealtimeThresholdLevel
{
    /// <summary>
    /// Liquidity spread and bid/ask updates, including lqSpread and related liquidity fields.
    /// </summary>
    LiquidityRiskMetric = 4,

    /// <summary>
    /// Consolidated reference price updates when Tiingo detects a meaningful consolidated reference price change.
    /// </summary>
    ReferencePrice = 6
}

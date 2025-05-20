using DotTiingo.Model.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Api.WebSocket;

/// <summary>
/// Provides access to the IEX WebSocket API.
/// </summary>
public interface ITiingoWebSocketIexApi
{
    /// <summary>
    /// Connects to the Tiingo IEX WebSocket API.
    /// </summary>
    /// <param name="thresholdLevel">The threshold level for the connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An <see cref="ITiingoWebSocketConnection"/> instance.</returns>
    Task<ITiingoWebSocketConnection> Connect(IexThresholdLevel thresholdLevel, CancellationToken cancellationToken);
}

/// <summary>
/// Implementation of <see cref="ITiingoWebSocketIexApi"/>.
/// </summary>
internal class WebSocketIexApi : ITiingoWebSocketIexApi
{
    private const string BaseUrl = $"{TiingoApiHelper.WebSocketBaseUrl}/iex";
    private readonly string _token;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketIexApi"/> class.
    /// </summary>
    /// <param name="token">The Tiingo API token.</param>
    public WebSocketIexApi(string token)
    {
        _token = token;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Connects to the Tiingo IEX WebSocket API.
    /// </summary>
    /// <param name="thresholdLevel">The threshold level for the connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An <see cref="ITiingoWebSocketConnection"/> instance.</returns>
    public Task<ITiingoWebSocketConnection> Connect(IexThresholdLevel thresholdLevel, CancellationToken cancellationToken)
    {
        var wsAuth = new WebSocketAuthorization("subscribe", _token, (int)thresholdLevel);
        var connFactory = new WebSocketConnectionFactory(wsAuth);
        return connFactory.CreateConnectionAsync(BaseUrl, cancellationToken);
    }
}

/// <summary>
/// Specifies the threshold level for the Tiingo IEX WebSocket feed, determining the type and amount of data received.
/// </summary>
public enum IexThresholdLevel
{
    /// <summary>
    /// Receive all Tiingo Reference price messages.
    /// </summary>
    ReferencePrice = 6,
    /// <summary>
    /// All updates from IEX. Be careful with this as it is A LOT of data.
    /// </summary>
    AllUpdates = 0,
    /// <summary>
    /// If a quote: sends updates where mid is not null AND there is a change in mid by at least $0.01 OR If a trade: send a trade if it differs from the last trade price.
    /// </summary>
    Filtered = 5
}

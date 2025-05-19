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
    Task<ITiingoWebSocketConnection> Connect(int thresholdLevel, CancellationToken cancellationToken);
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
    public Task<ITiingoWebSocketConnection> Connect(int thresholdLevel, CancellationToken cancellationToken)
    {
        var wsAuth = new WebSocketAuthorization("subscribe", _token, thresholdLevel);
        var connFactory = new WebSocketConnectionFactory(wsAuth);
        return connFactory.CreateConnectionAsync(BaseUrl, cancellationToken);
    }
}

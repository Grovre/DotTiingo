using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Api.WebSocket;

/// <summary>
/// Aggregates all Tiingo WebSocket API endpoints.
/// </summary>
public interface ITiingoWebSocketApi
{
    /// <summary>
    /// Provides access to the Crypto WebSocket API.
    /// </summary>
    ITiingoWebSocketCryptoApi Crypto { get; }
    /// <summary>
    /// Provides access to the Forex WebSocket API.
    /// </summary>
    ITiingoWebSocketForexApi Forex { get; }
    /// <summary>
    /// Provides access to the IEX WebSocket API.
    /// </summary>
    ITiingoWebSocketIexApi Iex { get; }
}

/// <summary>
/// Implementation of <see cref="ITiingoWebSocketApi"/> for accessing WebSocket endpoints.
/// </summary>
internal class WebSocketApi : ITiingoWebSocketApi
{
    /// <inheritdoc/>
    public ITiingoWebSocketCryptoApi Crypto { get; }
    /// <inheritdoc/>
    public ITiingoWebSocketForexApi Forex { get; }
    /// <inheritdoc/>
    public ITiingoWebSocketIexApi Iex { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketApi"/> class.
    /// </summary>
    /// <param name="token">The Tiingo API token.</param>
    public WebSocketApi(string token)
    {
        Crypto = new WebSocketCryptoApi(token);
        Forex = new WebSocketForexApi(token);
        Iex = new WebSocketIexApi(token);
    }
}

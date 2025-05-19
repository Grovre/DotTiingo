using DotTiingo.Api;
using DotTiingo.Api.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo;

/// <summary>
/// Main entry point for accessing Tiingo REST and WebSocket APIs.
/// </summary>
public class TiingoClient
{
    /// <summary>
    /// Provides access to the Tiingo REST API endpoints.
    /// </summary>
    public ITiingoRestApi Rest { get; }

    /// <summary>
    /// Provides access to the Tiingo WebSocket API endpoints.
    /// </summary>
    public ITiingoWebSocketApi WebSocket { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TiingoClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for REST API requests.</param>
    /// <param name="token">The Tiingo API token.</param>
    public TiingoClient(HttpClient httpClient, string token)
    {
        Rest = new RestApi(httpClient, token);
        WebSocket = new WebSocketApi(token);
    }
}

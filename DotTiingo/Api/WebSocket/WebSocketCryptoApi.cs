﻿using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotTiingo.Api.WebSocket;

/// <summary>
/// Provides access to the Crypto WebSocket API.
/// </summary>
public interface ITiingoWebSocketCryptoApi
{
    /// <summary>
    /// Connects to the Tiingo Crypto WebSocket API.
    /// </summary>
    /// <param name="thresholdLevel">The threshold level for the connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An <see cref="ITiingoWebSocketConnection"/> instance.</returns>
    Task<ITiingoWebSocketConnection> Connect(CryptoThresholdLevel thresholdLevel, CancellationToken cancellationToken);
}

/// <summary>
/// Implementation of <see cref="ITiingoWebSocketCryptoApi"/>.
/// </summary>
internal class WebSocketCryptoApi : ITiingoWebSocketCryptoApi
{
    private const string BaseUrl = $"{TiingoApiHelper.WebSocketBaseUrl}/crypto";
    private readonly string _token;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketCryptoApi"/> class.
    /// </summary>
    /// <param name="token">The Tiingo API token.</param>
    public WebSocketCryptoApi(string token)
    {
        _token = token;
    }

    /// <inheritdoc/>
    public Task<ITiingoWebSocketConnection> Connect(CryptoThresholdLevel thresholdLevel, CancellationToken cancellationToken)
    {
        var wsAuth = new WebSocketAuthorization("subscribe", _token, (int)thresholdLevel);
        var connFactory = new WebSocketConnectionFactory(wsAuth);
        return connFactory.CreateConnectionAsync(BaseUrl, cancellationToken);
    }
}

/// <summary>
/// Specifies the threshold level for the Tiingo Crypto WebSocket feed, determining the type and amount of data received.
/// </summary>
public enum CryptoThresholdLevel
{
    /// <summary>
    /// Top-of-Book quote updates as well as Trade updates. Both quote and trade updates are per-exchange.
    /// </summary>
    QuoteAndTrade = 2,
    /// <summary>
    /// Trade Updates per-exchange only.
    /// </summary>
    Trade = 5
}
using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotTiingo.Api.WebSocket;

public interface ITiingoWebSocketCryptoApi
{
    public Task<ITiingoWebSocketConnection> Connect(int thresholdLevel, CancellationToken cancellationToken);
}

internal class WebSocketCryptoApi : ITiingoWebSocketCryptoApi
{
    private const string BaseUrl = $"{TiingoApiHelper.WebSocketBaseUrl}/crypto";
    private readonly string _token;

    public WebSocketCryptoApi(string token)
    {
        _token = token;
    }

    public Task<ITiingoWebSocketConnection> Connect(int thresholdLevel, CancellationToken cancellationToken)
    {
        var wsAuth = new WebSocketAuthorization("subscribe", _token, thresholdLevel);
        var connFactory = new WebSocketConnectionFactory(wsAuth);
        return connFactory.CreateConnectionAsync(BaseUrl, cancellationToken);
    }
}

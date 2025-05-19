using DotTiingo.Model.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Api.WebSocket;

public interface ITiingoWebSocketIexApi
{
    Task<ITiingoWebSocketConnection> Connect(int thresholdLevel, CancellationToken cancellationToken);
}

internal class WebSocketIexApi : ITiingoWebSocketIexApi
{
    private const string BaseUrl = $"{TiingoApiHelper.WebSocketBaseUrl}/iex";
    private readonly string _token;

    public WebSocketIexApi(string token)
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

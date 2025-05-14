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

    public async Task<ITiingoWebSocketConnection> Connect(int thresholdLevel, CancellationToken cancellationToken)
    {
        var cws = new ClientWebSocket();
        await cws.ConnectAsync(new(BaseUrl), CancellationToken.None);

        var wsAuth = new WebSocketAuthorization("subscribe", _token, thresholdLevel);
        var authJson = JsonSerializer.Serialize(wsAuth, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var authBytes = Encoding.UTF8.GetBytes(authJson);

        var conn = new WebSocketConnection(cws, cancellationToken);
        EventHandler<AbstractResponse> authCheck = (object? _, AbstractResponse response) =>
        {
            if (response is not UtilityResponse utilityResponse)
                throw new Exception(
                    "WebSocket authorization response was not of type UtilityResponse");

            if (utilityResponse.ResponseCode != 200 || utilityResponse.MessageType == 'E')
                throw new Exception(
                    $"WebSocket authorization failed with code {utilityResponse.ResponseCode} and message: {utilityResponse.ResponseMessage}");

            if (utilityResponse.MessageType != 'I')
                throw new Exception(
            $"WebSocket authorization response was not of type 'I' (info) but {utilityResponse.MessageType}");
        };
        authCheck += (sender, _) => ((ITiingoWebSocketConnection)sender).OnResponseReceived -= authCheck;
        conn.OnResponseReceived += authCheck;
        await cws.SendAsync(authBytes, WebSocketMessageType.Text, true, cancellationToken);
        return conn;
    }
}

using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DotTiingo.Api.WebSocket;

internal class WebSocketConnectionFactory(WebSocketAuthorization wsAuth)
{
    public async Task<ITiingoWebSocketConnection> CreateConnectionAsync(string baseUrl, CancellationToken cancelToken)
    {
        var authJson = JsonSerializer.Serialize(wsAuth, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var authBytes = Encoding.UTF8.GetBytes(authJson);

        var cws = new ClientWebSocket();
        await cws.ConnectAsync(new(baseUrl), cancelToken);
        var conn = new WebSocketConnection(cws, cancelToken);
        EventHandler<AbstractResponse> authCheck = (_, response) =>
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
        await cws.SendAsync(authBytes, WebSocketMessageType.Text, true, cancelToken);
        return conn;
    }
}

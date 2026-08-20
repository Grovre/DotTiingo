using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DotTiingo.Api.WebSocket;

internal class WebSocketConnectionFactory(WebSocketAuthorization wsAuth, TimeSpan? authTimeout = null)
{
    private static readonly TimeSpan DefaultAuthTimeout = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _authTimeout = authTimeout ?? DefaultAuthTimeout;

    public async Task<ITiingoWebSocketConnection> CreateConnectionAsync(string baseUrl, CancellationToken cancelToken)
    {
        var authJson = JsonSerializer.Serialize(wsAuth, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var authBytes = Encoding.UTF8.GetBytes(authJson);

        var cws = new ClientWebSocket();
        WebSocketConnection? conn = null;
        try
        {
            await cws.ConnectAsync(new Uri(baseUrl), cancelToken);
            conn = new WebSocketConnection(cws, cancelToken);

            await AuthenticateAsync(conn, cws, authBytes, cancelToken);
            return conn;
        }
        catch
        {
            conn?.Dispose();
            cws.Dispose();
            throw;
        }
    }

    internal async Task AuthenticateAsync(WebSocketConnection conn, System.Net.WebSockets.WebSocket ws, byte[] authBytes, CancellationToken cancelToken)
    {
        var authTcs = new TaskCompletionSource<AbstractResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnResponse(object? sender, AbstractResponse response)
        {
            authTcs.TrySetResult(response);
        }

        conn.OnResponseReceived += OnResponse;

        using var timeoutCts = new CancellationTokenSource(_authTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, timeoutCts.Token);

        using var registration = linkedCts.Token.Register(() =>
        {
            if (cancelToken.IsCancellationRequested)
            {
                authTcs.TrySetCanceled(cancelToken);
            }
            else if (timeoutCts.IsCancellationRequested)
            {
                authTcs.TrySetException(new TimeoutException("WebSocket authorization timed out. No responses received."));
            }
        });

        using var connRegistration = conn.CancellationToken.Register(() =>
        {
            if (conn.SurfacedException != null)
            {
                authTcs.TrySetException(conn.SurfacedException);
            }
            else if (cancelToken.IsCancellationRequested)
            {
                authTcs.TrySetCanceled(cancelToken);
            }
            else
            {
                authTcs.TrySetException(new InvalidOperationException("WebSocket connection was closed or cancelled before authorization completed."));
            }
        });

        try
        {
            await ws.SendAsync(authBytes, WebSocketMessageType.Text, true, cancelToken);

            var response = await authTcs.Task;

            if (response is not UtilityResponse utilityResponse)
            {
                throw new InvalidOperationException(
                    "WebSocket authorization response was not of type UtilityResponse");
            }

            if (utilityResponse.ResponseCode != 200 || utilityResponse.MessageType == 'E')
            {
                throw new InvalidOperationException(
                    $"WebSocket authorization failed with code {utilityResponse.ResponseCode} and message: {utilityResponse.ResponseMessage}");
            }

            if (utilityResponse.MessageType != 'I')
            {
                throw new InvalidOperationException(
                    $"WebSocket authorization response was not of type 'I' (info) but {utilityResponse.MessageType}");
            }
        }
        finally
        {
            conn.OnResponseReceived -= OnResponse;
        }
    }
}
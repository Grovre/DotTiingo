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
        var authTimeoutCheck = new AuthTimeoutCheck(TimeSpan.FromSeconds(10));
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
        authCheck += (_, _) => authTimeoutCheck.TimedOut = false;
        authCheck += (sender, _) => ((ITiingoWebSocketConnection)sender!).OnResponseReceived -= authCheck;
        conn.OnResponseReceived += authCheck;
        await cws.SendAsync(authBytes, WebSocketMessageType.Text, true, cancelToken);
        await authTimeoutCheck.CreateAuthCheckTask();
        return conn;
    }
}

/// <summary>
/// Provides a mechanism to enforce a timeout for WebSocket authorization.
/// Throws a <see cref="TimeoutException"/> if authorization is not completed within the specified time span.
/// </summary>
file sealed class AuthTimeoutCheck
{
    private readonly CancellationTokenSource _cancelTokenSource;
    private bool _timedOut;

    /// <summary>
    /// Gets or sets a value indicating whether the authorization has timed out.
    /// Setting this to <c>false</c> cancels the timeout check.
    /// </summary>
    public bool TimedOut
    {
        get => _timedOut;
        set
        {
            _timedOut = value;
            if (!value)
                _cancelTokenSource.Cancel();
        }
    }

    /// <summary>
    /// Gets the time span to wait before timing out the authorization.
    /// </summary>
    public TimeSpan TimeSpan { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthTimeoutCheck"/> class with the specified timeout duration.
    /// </summary>
    /// <param name="timeSpan">The duration to wait before timing out.</param>
    public AuthTimeoutCheck(TimeSpan timeSpan)
    {
        _cancelTokenSource = new();
        TimedOut = true;
        TimeSpan = timeSpan;
    }

    /// <summary>
    /// Starts the authorization timeout check. Throws a <see cref="TimeoutException"/> if the timeout elapses before authorization completes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="TimeoutException">Thrown if the timeout elapses before authorization completes.</exception>
    public async Task CreateAuthCheckTask()
    {
        if (!TimedOut || _cancelTokenSource.IsCancellationRequested)
            return;

        try
        {
            await Task.Delay(TimeSpan, _cancelTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Task was canceled, do nothing
        }

        if (TimedOut)
            throw new TimeoutException("WebSocket authorization timed out. No responses received.");
    }
}
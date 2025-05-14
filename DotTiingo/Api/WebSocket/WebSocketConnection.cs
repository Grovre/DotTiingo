using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotTiingo.Api.WebSocket;

public interface ITiingoWebSocketConnection : IDisposable
{
    public event EventHandler<AbstractResponse>? OnResponseReceived;
}

internal sealed class WebSocketConnection : ITiingoWebSocketConnection
{
    private readonly ClientWebSocket _clientWebSocket;
    private readonly CancellationTokenSource _cancelTokenSource;
    public Task ReceiveTask { get; private set; }
    public event EventHandler<AbstractResponse>? OnResponseReceived;
    private bool _readingSocket = false;

    public WebSocketConnection(ClientWebSocket clientWebSocket, CancellationToken cancellationToken)
    {
        _clientWebSocket = clientWebSocket;
        _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellationToken = _cancelTokenSource.Token;
        ReceiveTask = new TaskFactory().StartNew(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var response = ReceiveAsync(cancellationToken).GetAwaiter().GetResult();
                OnResponseReceived?.Invoke(this, response);
            }
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async Task<AbstractResponse> ReceiveAsync(CancellationToken cancellationToken)
    {
        _readingSocket = true;
        var buffer = new byte[1024];
        var appendBuffer = new List<byte>();
        WebSocketReceiveResult receiveResult;
        do
        {
            receiveResult = await _clientWebSocket.ReceiveAsync(buffer, cancellationToken);
            appendBuffer.AddRange(buffer.AsSpan(0, receiveResult.Count));
        } while (!receiveResult.EndOfMessage);
        _readingSocket = false;

        var bytes = CollectionsMarshal.AsSpan(appendBuffer);
        var json = Encoding.UTF8.GetString(bytes);

        var responseFactory = new ResponseFactory();
        var response = responseFactory.CreateResponseFromJson(json)
            ?? throw new NullReferenceException(
                "WebSocket response was deserialized as null");
        if (response.MessageType == 'E')
            throw new InvalidOperationException(
                $"WebSocket error:\n{json}");

        return response;
    }

    public void Dispose()
    {
        _cancelTokenSource.Cancel();
        _cancelTokenSource.Dispose();
        _clientWebSocket.Dispose();
        GC.SuppressFinalize(this);
    }
}

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

    public WebSocketConnection(ClientWebSocket clientWebSocket, CancellationToken cancellationToken)
    {
        _clientWebSocket = clientWebSocket;
        _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        ReceiveTask = ReceiveLoopAsync(_cancelTokenSource.Token);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var response = await ReceiveAsync(cancellationToken);
            OnResponseReceived?.Invoke(this, response);
        }
    }

    private readonly byte[] _buffer = new byte[1024];
    private readonly List<byte> _appendBuffer = [];
    private readonly ResponseFactory _responseFactory = new();
    private async Task<AbstractResponse> ReceiveAsync(CancellationToken cancellationToken)
    {
        _appendBuffer.Clear();
        Span<byte> bytes;
        WebSocketReceiveResult receiveResult;
        var firstReceive = true;
        do
        {
            receiveResult = await _clientWebSocket.ReceiveAsync(_buffer, cancellationToken);

            if (firstReceive && receiveResult.EndOfMessage)
            {
                bytes = _buffer.AsSpan(0, receiveResult.Count);
                break;
            }

            firstReceive = false;
            _appendBuffer.AddRange(_buffer.AsSpan(0, receiveResult.Count));
        } while (true);

        if (!firstReceive)
            bytes = CollectionsMarshal.AsSpan(_appendBuffer);

        var json = Encoding.UTF8.GetString(bytes);
        var response = _responseFactory.CreateResponseFromJson(json)
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

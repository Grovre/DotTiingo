using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DotTiingo.Api.WebSocket;

/// <summary>
/// Represents a connection to a Tiingo WebSocket API endpoint.
/// </summary>
public interface ITiingoWebSocketConnection : IDisposable
{
    /// <summary>
    /// Occurs when a response is received from the WebSocket connection.
    /// </summary>
    public event EventHandler<AbstractResponse>? OnResponseReceived;

    /// <summary>
    /// Receives responses from the WebSocket connection as an asynchronous stream.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous stream.</param>
    /// <returns>An <see cref="IAsyncEnumerable{AbstractResponse}"/> of responses received from the WebSocket connection.</returns>
    public IAsyncEnumerable<AbstractResponse> ReceiveEnumerableAsync(CancellationToken cancellationToken = default);
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

    private Exception? _surfacedException = null;
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await ReceiveAsync(cancellationToken);
                OnResponseReceived?.Invoke(this, response);
            }
        }

        catch (Exception ex)
        {
            if (ex is not OperationCanceledException)
                _surfacedException = ex;
            
            await _cancelTokenSource.CancelAsync();
        }
    }

    public IAsyncEnumerable<AbstractResponse> ReceiveEnumerableAsync(CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<AbstractResponse>(new UnboundedChannelOptions
        {
            SingleWriter = true,
            SingleReader = true
        });
        var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancelTokenSource.Token);

        void ReceiveFn(object? sender, AbstractResponse response)
        {
            channel.Writer.TryWrite(response);
        }

        OnResponseReceived += ReceiveFn;
        cancellationTokens.Token.Register(() =>
        {
            OnResponseReceived -= ReceiveFn;
            if (_surfacedException == null)
            {
                channel.Writer.Complete();
            }
            else
            {
                channel.Writer.Complete(_surfacedException);
            }
            cancellationTokens.Dispose();
        });

        return channel.Reader.ReadAllAsync(cancellationTokens.Token);
    }

    private const int ReceiveChunkSize = 4096;
    private readonly ArrayBufferWriter<byte> _buffer = new(ReceiveChunkSize);
    private async Task<AbstractResponse> ReceiveAsync(CancellationToken cancellationToken)
    {
        _buffer.ResetWrittenCount();
        var endOfMessage = false;
        while (!endOfMessage)
        {
            // Receive a chunk of data from the WebSocket. The size hint matters: without it
            // GetMemory only guarantees a single byte, so a fragmented message would be
            // received through an ever-shrinking window as the buffer fills.
            var receiveResult = await _clientWebSocket.ReceiveAsync(
                _buffer.GetMemory(ReceiveChunkSize), cancellationToken);
            _buffer.Advance(receiveResult.Count);
            endOfMessage = receiveResult.EndOfMessage;
        }

        var response = ResponseFactory.CreateResponseFromJson(_buffer.WrittenSpan);
        // Tiingo reports its own failures as an 'E' frame. Surface the code, the message and
        // the raw frame together, since that is all the caller has to diagnose with.
        if (response is UtilityResponse { MessageType: 'E' } error)
            throw new InvalidOperationException(
                $"WebSocket error {error.ResponseCode}: {error.ResponseMessage}\n"
                + Encoding.UTF8.GetString(_buffer.WrittenSpan));

        return response;
    }

    private bool _disposed = false;
    public void Dispose()
    {
        if (_disposed)
            return;

        _cancelTokenSource.Cancel();
        _cancelTokenSource.Dispose();
        _clientWebSocket.Dispose();
        GC.SuppressFinalize(this);

        _disposed = true;
    }
}

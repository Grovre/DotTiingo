using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;

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
    private readonly System.Net.WebSockets.WebSocket _clientWebSocket;
    private readonly CancellationTokenSource _cancelTokenSource;
    public Task ReceiveTask { get; private set; }
    public event EventHandler<AbstractResponse>? OnResponseReceived;

    internal Exception? SurfacedException => _surfacedException;
    internal CancellationToken CancellationToken => _cancelTokenSource.Token;

    public WebSocketConnection(System.Net.WebSockets.WebSocket clientWebSocket, CancellationToken cancellationToken)
    {
        _clientWebSocket = clientWebSocket;
        _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        ReceiveTask = ReceiveLoopAsync(_cancelTokenSource.Token);
    }

    private Exception? _surfacedException;
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

    public async IAsyncEnumerable<AbstractResponse> ReceiveEnumerableAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
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
        using var registration = cancellationTokens.Token.Register(() =>
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
        });

        try
        {
            await foreach (var response in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return response;
            }
        }
        finally
        {
            OnResponseReceived -= ReceiveFn;
            cancellationTokens.Dispose();
        }
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

            if (receiveResult.MessageType == WebSocketMessageType.Close)
            {
                throw new WebSocketException(WebSocketError.ConnectionClosedPrematurely, "WebSocket connection was closed by the remote endpoint.");
            }

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

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed)
            return;

        _cancelTokenSource.Cancel();
        _cancelTokenSource.Dispose();
        _clientWebSocket.Dispose();
        // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
        GC.SuppressFinalize(this);

        _disposed = true;
    }
}

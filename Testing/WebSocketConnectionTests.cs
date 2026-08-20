using DotTiingo.Api.WebSocket;
using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using NUnit.Framework;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;

namespace Testing;

[TestFixture]
public class WebSocketConnectionTests
{
    private sealed class FakeWebSocket : WebSocket
    {
        private readonly Channel<Func<Memory<byte>, CancellationToken, ValueTask<ValueWebSocketReceiveResult>>> _receiveChannel =
            Channel.CreateUnbounded<Func<Memory<byte>, CancellationToken, ValueTask<ValueWebSocketReceiveResult>>>();

        public List<string> SentMessages { get; } = [];
        public bool Disposed { get; private set; }
        private WebSocketState _state = WebSocketState.Open;

        public override WebSocketCloseStatus? CloseStatus => null;
        public override string? CloseStatusDescription => null;
        public override WebSocketState State => _state;
        public override string? SubProtocol => null;

        public void EnqueueMessage(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            _receiveChannel.Writer.TryWrite((buffer, _) =>
            {
                bytes.AsSpan().CopyTo(buffer.Span);
                return ValueTask.FromResult(new ValueWebSocketReceiveResult(bytes.Length, WebSocketMessageType.Text, true));
            });
        }

        public void EnqueueException(Exception ex)
        {
            _receiveChannel.Writer.TryWrite((_, _) => throw ex);
        }

        public void EnqueueClose()
        {
            _receiveChannel.Writer.TryWrite((_, _) =>
                ValueTask.FromResult(new ValueWebSocketReceiveResult(0, WebSocketMessageType.Close, true)));
        }

        public override void Abort()
        {
            _state = WebSocketState.Aborted;
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            _state = WebSocketState.Closed;
            return Task.CompletedTask;
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            _state = WebSocketState.CloseSent;
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            Disposed = true;
            _state = WebSocketState.Closed;
        }

        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Use ValueWebSocketReceiveResult overload");
        }

        public override async ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            var handler = await _receiveChannel.Reader.ReadAsync(cancellationToken);
            return await handler(buffer, cancellationToken);
        }

        public Action<string>? OnSend { get; set; }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            var msg = Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count);
            SentMessages.Add(msg);
            OnSend?.Invoke(msg);
            return Task.CompletedTask;
        }

        public override ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            var msg = Encoding.UTF8.GetString(buffer.Span);
            SentMessages.Add(msg);
            OnSend?.Invoke(msg);
            return ValueTask.CompletedTask;
        }
    }

    [Test]
    public async Task AuthenticateAsync_ValidUtilityInfoResponse_Succeeds()
    {
        var fakeWs = new FakeWebSocket();
        fakeWs.OnSend = _ => fakeWs.EnqueueMessage("""
        {
            "messageType": "I",
            "response": {
                "code": 200,
                "message": "Success"
            }
        }
        """);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var conn = new WebSocketConnection(fakeWs, cts.Token);
        var auth = new WebSocketAuthorization("subscribe", "test-token", 2);
        var factory = new WebSocketConnectionFactory(auth, TimeSpan.FromSeconds(2));

        var authBytes = Encoding.UTF8.GetBytes("{\"eventName\":\"subscribe\"}");
        await factory.AuthenticateAsync(conn, fakeWs, authBytes, cts.Token);

        Assert.That(fakeWs.SentMessages, Has.Count.EqualTo(1));
        Assert.That(fakeWs.SentMessages[0], Is.EqualTo("{\"eventName\":\"subscribe\"}"));
    }

    [Test]
    public void AuthenticateAsync_ErrorFrame_ThrowsImmediatelyWithErrorCodeAndMessage()
    {
        var fakeWs = new FakeWebSocket();
        fakeWs.OnSend = _ => fakeWs.EnqueueMessage("""
        {
            "messageType": "E",
            "response": {
                "code": 400,
                "message": "Invalid token"
            }
        }
        """);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var conn = new WebSocketConnection(fakeWs, cts.Token);
        var auth = new WebSocketAuthorization("subscribe", "test-token", 2);
        var factory = new WebSocketConnectionFactory(auth, TimeSpan.FromSeconds(5));

        var authBytes = Encoding.UTF8.GetBytes("{\"eventName\":\"subscribe\"}");
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await factory.AuthenticateAsync(conn, fakeWs, authBytes, cts.Token);
        });

        Assert.That(ex!.Message, Does.Contain("WebSocket error 400: Invalid token"));
    }

    [Test]
    public void AuthenticateAsync_Non200ResponseCode_ThrowsInvalidOperationException()
    {
        var fakeWs = new FakeWebSocket();
        fakeWs.OnSend = _ => fakeWs.EnqueueMessage("""
        {
            "messageType": "I",
            "response": {
                "code": 403,
                "message": "Forbidden"
            }
        }
        """);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var conn = new WebSocketConnection(fakeWs, cts.Token);
        var auth = new WebSocketAuthorization("subscribe", "test-token", 2);
        var factory = new WebSocketConnectionFactory(auth, TimeSpan.FromSeconds(5));

        var authBytes = Encoding.UTF8.GetBytes("{\"eventName\":\"subscribe\"}");
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await factory.AuthenticateAsync(conn, fakeWs, authBytes, cts.Token);
        });

        Assert.That(ex!.Message, Does.Contain("WebSocket authorization failed with code 403"));
    }

    [Test]
    public void AuthenticateAsync_UnexpectedMessageType_ThrowsInvalidOperationException()
    {
        var fakeWs = new FakeWebSocket();
        fakeWs.OnSend = _ => fakeWs.EnqueueMessage("""
        {
            "messageType": "H",
            "response": {
                "code": 200,
                "message": "Heartbeat"
            }
        }
        """);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var conn = new WebSocketConnection(fakeWs, cts.Token);
        var auth = new WebSocketAuthorization("subscribe", "test-token", 2);
        var factory = new WebSocketConnectionFactory(auth, TimeSpan.FromSeconds(5));

        var authBytes = Encoding.UTF8.GetBytes("{\"eventName\":\"subscribe\"}");
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await factory.AuthenticateAsync(conn, fakeWs, authBytes, cts.Token);
        });

        Assert.That(ex!.Message, Does.Contain("WebSocket authorization response was not of type 'I' (info) but H"));
    }

    [Test]
    public void AuthenticateAsync_Timeout_ThrowsTimeoutException()
    {
        var fakeWs = new FakeWebSocket();
        // Do not enqueue any message to trigger timeout

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var conn = new WebSocketConnection(fakeWs, cts.Token);
        var auth = new WebSocketAuthorization("subscribe", "test-token", 2);
        var factory = new WebSocketConnectionFactory(auth, TimeSpan.FromMilliseconds(100));

        var authBytes = Encoding.UTF8.GetBytes("{\"eventName\":\"subscribe\"}");
        var ex = Assert.ThrowsAsync<TimeoutException>(async () =>
        {
            await factory.AuthenticateAsync(conn, fakeWs, authBytes, cts.Token);
        });

        Assert.That(ex!.Message, Does.Contain("WebSocket authorization timed out"));
    }

    [Test]
    public void AuthenticateAsync_CancellationTokenCancelled_ThrowsOperationCanceledException()
    {
        var fakeWs = new FakeWebSocket();
        using var cts = new CancellationTokenSource();

        using var conn = new WebSocketConnection(fakeWs, cts.Token);
        var auth = new WebSocketAuthorization("subscribe", "test-token", 2);
        var factory = new WebSocketConnectionFactory(auth, TimeSpan.FromSeconds(5));

        var authBytes = Encoding.UTF8.GetBytes("{\"eventName\":\"subscribe\"}");
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await factory.AuthenticateAsync(conn, fakeWs, authBytes, cts.Token);
        });
    }

    [Test]
    public void AuthenticateAsync_ConnectionDropped_ThrowsImmediately()
    {
        var fakeWs = new FakeWebSocket();
        fakeWs.EnqueueException(new WebSocketException(WebSocketError.ConnectionClosedPrematurely, "Connection lost"));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var conn = new WebSocketConnection(fakeWs, cts.Token);
        var auth = new WebSocketAuthorization("subscribe", "test-token", 2);
        var factory = new WebSocketConnectionFactory(auth, TimeSpan.FromSeconds(5));

        var authBytes = Encoding.UTF8.GetBytes("{\"eventName\":\"subscribe\"}");
        var ex = Assert.ThrowsAsync<WebSocketException>(async () =>
        {
            await factory.AuthenticateAsync(conn, fakeWs, authBytes, cts.Token);
        });

        Assert.That(ex!.Message, Does.Contain("Connection lost"));
    }

    [Test]
    public void AuthenticateAsync_RemoteClose_ThrowsWebSocketException()
    {
        var fakeWs = new FakeWebSocket();
        fakeWs.EnqueueClose();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var conn = new WebSocketConnection(fakeWs, cts.Token);
        var auth = new WebSocketAuthorization("subscribe", "test-token", 2);
        var factory = new WebSocketConnectionFactory(auth, TimeSpan.FromSeconds(5));

        var authBytes = Encoding.UTF8.GetBytes("{\"eventName\":\"subscribe\"}");
        var ex = Assert.ThrowsAsync<WebSocketException>(async () =>
        {
            await factory.AuthenticateAsync(conn, fakeWs, authBytes, cts.Token);
        });

        Assert.That(ex!.WebSocketErrorCode, Is.EqualTo(WebSocketError.ConnectionClosedPrematurely));
    }

    [Test]
    public void ReceiveEnumerableAsync_PropagatesSurfacedException()
    {
        var fakeWs = new FakeWebSocket();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var conn = new WebSocketConnection(fakeWs, cts.Token);

        var receivedCount = 0;
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var consumeTask = Task.Run(async () =>
            {
                await foreach (var response in conn.ReceiveEnumerableAsync(cts.Token))
                {
                    receivedCount++;
                }
            });

            // Allow consumeTask to enter ReceiveEnumerableAsync and subscribe to OnResponseReceived
            await Task.Delay(50);

            fakeWs.EnqueueMessage("""
            {
                "messageType": "A",
                "service": "crypto_data",
                "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25]
            }
            """);

            fakeWs.EnqueueMessage("""
            {
                "messageType": "E",
                "response": {
                    "code": 401,
                    "message": "Unauthorized"
                }
            }
            """);

            await consumeTask;
        });

        Assert.That(receivedCount, Is.EqualTo(1));
        Assert.That(ex!.Message, Does.Contain("WebSocket error 401: Unauthorized"));
    }

    [Test]
    public void AuthenticateAsync_NonUtilityResponse_ThrowsInvalidOperationException()
    {
        var fakeWs = new FakeWebSocket();
        fakeWs.OnSend = _ => fakeWs.EnqueueMessage("""
        {
            "messageType": "A",
            "service": "crypto_data",
            "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25]
        }
        """);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var conn = new WebSocketConnection(fakeWs, cts.Token);
        var auth = new WebSocketAuthorization("subscribe", "test-token", 2);
        var factory = new WebSocketConnectionFactory(auth, TimeSpan.FromSeconds(5));

        var authBytes = Encoding.UTF8.GetBytes("{\"eventName\":\"subscribe\"}");
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await factory.AuthenticateAsync(conn, fakeWs, authBytes, cts.Token);
        });

        Assert.That(ex!.Message, Does.Contain("WebSocket authorization response was not of type UtilityResponse"));
    }

    [Test]
    public void CreateConnectionAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var auth = new WebSocketAuthorization("subscribe", "test-token", 2);
        var factory = new WebSocketConnectionFactory(auth);

        Assert.CatchAsync<OperationCanceledException>(async () =>
        {
            await factory.CreateConnectionAsync("wss://localhost:12345/test", cts.Token);
        });
    }

    [Test]
    public async Task ReceiveEnumerableAsync_CleanCancellation_CompletesStream()
    {
        var fakeWs = new FakeWebSocket();
        using var connCts = new CancellationTokenSource();
        using var conn = new WebSocketConnection(fakeWs, connCts.Token);
        using var streamCts = new CancellationTokenSource();

        var received = new List<AbstractResponse>();
        var readTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var response in conn.ReceiveEnumerableAsync(streamCts.Token))
                {
                    received.Add(response);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when streamCts is canceled
            }
        });

        await Task.Delay(50);

        fakeWs.EnqueueMessage("""
        {
            "messageType": "A",
            "service": "crypto_data",
            "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25]
        }
        """);

        await Task.Delay(50);
        await streamCts.CancelAsync();
        await readTask;

        Assert.That(received, Has.Count.EqualTo(1));
    }

    [Test]
    public void WebSocketConnection_Dispose_IsIdempotentAndDisposesUnderlyingWebSocket()
    {
        var fakeWs = new FakeWebSocket();
        using var cts = new CancellationTokenSource();
        var conn = new WebSocketConnection(fakeWs, cts.Token);

        Assert.That(fakeWs.Disposed, Is.False);
        conn.Dispose();
        Assert.That(fakeWs.Disposed, Is.True);
        Assert.DoesNotThrow(() => conn.Dispose());
    }
}

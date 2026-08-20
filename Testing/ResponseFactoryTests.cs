using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using NUnit.Framework;
using System;
using System.Text;
using System.Text.Json;

namespace Testing;

[TestFixture]
public class ResponseFactoryTests
{
    [Test]
    public void CreateResponseFromJson_CryptoTradeUpdate_ReturnsDataResponse()
    {
        var json = """
        {
            "messageType": "A",
            "service": "crypto_data",
            "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25]
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<DataResponse>());
        var dataResponse = (DataResponse)response;
        Assert.That(dataResponse.MessageType, Is.EqualTo('A'));
        Assert.That(dataResponse.Service, Is.EqualTo("crypto_data"));
        Assert.That(dataResponse.Data, Is.InstanceOf<CryptoTradeUpdate>());

        var trade = (CryptoTradeUpdate)dataResponse.Data;
        Assert.That(trade.UpdateMessageType, Is.EqualTo('T'));
        Assert.That(trade.Ticker, Is.EqualTo("btcusd"));
        Assert.That(trade.Date, Is.EqualTo(DateTimeOffset.Parse("2019-01-29T19:35:10.923490+00:00")));
        Assert.That(trade.Exchange, Is.EqualTo("binance"));
        Assert.That(trade.LastSize, Is.EqualTo(0.05f).Within(0.0001f));
        Assert.That(trade.LastPrice, Is.EqualTo(3450.25f).Within(0.0001f));
    }

    [Test]
    public void CreateResponseFromJson_CryptoQuoteUpdate_ReturnsDataResponse()
    {
        var json = """
        {
            "messageType": "A",
            "service": "crypto_data",
            "data": ["Q", "ethusd", "2023-05-10T12:00:00.000000+00:00", "coinbase", 1.5, 1800.1, 1800.3, 2.0, 1800.5]
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<DataResponse>());
        var dataResponse = (DataResponse)response;
        Assert.That(dataResponse.MessageType, Is.EqualTo('A'));
        Assert.That(dataResponse.Service, Is.EqualTo("crypto_data"));
        Assert.That(dataResponse.Data, Is.InstanceOf<CryptoQuoteUpdate>());

        var quote = (CryptoQuoteUpdate)dataResponse.Data;
        Assert.That(quote.UpdateMessageType, Is.EqualTo('Q'));
        Assert.That(quote.Ticker, Is.EqualTo("ethusd"));
        Assert.That(quote.Date, Is.EqualTo(DateTimeOffset.Parse("2023-05-10T12:00:00.000000+00:00")));
        Assert.That(quote.Exchange, Is.EqualTo("coinbase"));
        Assert.That(quote.BidSize, Is.EqualTo(1.5f).Within(0.0001f));
        Assert.That(quote.BidPrice, Is.EqualTo(1800.1f).Within(0.0001f));
        Assert.That(quote.MidPrice, Is.EqualTo(1800.3f).Within(0.0001f));
        Assert.That(quote.AskSize, Is.EqualTo(2.0f).Within(0.0001f));
        Assert.That(quote.AskPrice, Is.EqualTo(1800.5f).Within(0.0001f));
    }

    [Test]
    public void CreateResponseFromJson_IexReferencePriceUpdate_ReturnsDataResponse()
    {
        var json = """
        {
            "messageType": "A",
            "service": "iex",
            "data": ["2023-01-15T15:30:00.000000+00:00", "AAPL", 150.75]
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<DataResponse>());
        var dataResponse = (DataResponse)response;
        Assert.That(dataResponse.MessageType, Is.EqualTo('A'));
        Assert.That(dataResponse.Service, Is.EqualTo("iex"));
        Assert.That(dataResponse.Data, Is.InstanceOf<IexReferencePriceUpdate>());

        var iex = (IexReferencePriceUpdate)dataResponse.Data;
        Assert.That(iex.Date, Is.EqualTo(DateTimeOffset.Parse("2023-01-15T15:30:00.000000+00:00")));
        Assert.That(iex.Ticker, Is.EqualTo("AAPL"));
        Assert.That(iex.ReferencePrice, Is.EqualTo(150.75f).Within(0.0001f));
    }

    [Test]
    public void CreateResponseFromJson_ForexQuoteUpdate_ReturnsDataResponse()
    {
        var json = """
        {
            "messageType": "A",
            "service": "fx",
            "data": ["Q", "eurusd", "2023-08-01T10:15:30.500000+00:00", 1000000, 1.0950, 1.0951, 1000000, 1.0952]
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<DataResponse>());
        var dataResponse = (DataResponse)response;
        Assert.That(dataResponse.MessageType, Is.EqualTo('A'));
        Assert.That(dataResponse.Service, Is.EqualTo("fx"));
        Assert.That(dataResponse.Data, Is.InstanceOf<ForexQuoteUpdate>());

        var fx = (ForexQuoteUpdate)dataResponse.Data;
        Assert.That(fx.UpdateMessageType, Is.EqualTo('Q'));
        Assert.That(fx.Ticker, Is.EqualTo("eurusd"));
        Assert.That(fx.Date, Is.EqualTo(DateTimeOffset.Parse("2023-08-01T10:15:30.500000+00:00")));
        Assert.That(fx.BidSize, Is.EqualTo(1000000f).Within(0.0001f));
        Assert.That(fx.BidPrice, Is.EqualTo(1.0950f).Within(0.0001f));
        Assert.That(fx.MidPrice, Is.EqualTo(1.0951f).Within(0.0001f));
        Assert.That(fx.AskSize, Is.EqualTo(1000000f).Within(0.0001f));
        Assert.That(fx.AskPrice, Is.EqualTo(1.0952f).Within(0.0001f));
    }

    [Test]
    public void CreateResponseFromJson_UtilityInfoResponse_ReturnsUtilityResponse()
    {
        var json = """
        {
            "messageType": "I",
            "response": {
                "code": 200,
                "message": "Connected successfully"
            },
            "data": {
                "subscriptionId": 42
            }
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<UtilityResponse>());
        var utility = (UtilityResponse)response;
        Assert.That(utility.MessageType, Is.EqualTo('I'));
        Assert.That(utility.ResponseCode, Is.EqualTo(200));
        Assert.That(utility.ResponseMessage, Is.EqualTo("Connected successfully"));
        Assert.That(utility.SubscriptionId, Is.EqualTo(42));
    }

    [Test]
    public void CreateResponseFromJson_HeartbeatResponse_ReturnsUtilityResponse()
    {
        var json = """
        {
            "messageType": "H",
            "response": {
                "code": 200,
                "message": "Heartbeat"
            }
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<UtilityResponse>());
        var utility = (UtilityResponse)response;
        Assert.That(utility.MessageType, Is.EqualTo('H'));
        Assert.That(utility.ResponseCode, Is.EqualTo(200));
        Assert.That(utility.ResponseMessage, Is.EqualTo("Heartbeat"));
        Assert.That(utility.SubscriptionId, Is.Null);
    }

    [Test]
    public void CreateResponseFromJson_KeysInDifferentOrder_ParsesCorrectly()
    {
        var json = """
        {
            "service": "crypto_data",
            "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25],
            "messageType": "A"
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<DataResponse>());
        var dataResponse = (DataResponse)response;
        Assert.That(dataResponse.MessageType, Is.EqualTo('A'));
        Assert.That(dataResponse.Service, Is.EqualTo("crypto_data"));
        Assert.That(dataResponse.Data, Is.InstanceOf<CryptoTradeUpdate>());
    }

    [Test]
    public void CreateResponseFromJson_DataBeforeService_ParsesCorrectly()
    {
        var json = """
        {
            "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25],
            "service": "crypto_data",
            "messageType": "A"
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<DataResponse>());
        var dataResponse = (DataResponse)response;
        Assert.That(dataResponse.MessageType, Is.EqualTo('A'));
        Assert.That(dataResponse.Service, Is.EqualTo("crypto_data"));
        Assert.That(dataResponse.Data, Is.InstanceOf<CryptoTradeUpdate>());
    }

    [Test]
    public void CreateResponseFromJson_UnsupportedMessageType_ThrowsNotSupportedException()
    {
        var json = """
        {
            "messageType": "Z"
        }
        """;

        Assert.Throws<NotSupportedException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_UnsupportedCryptoUpdateType_ThrowsNotSupportedException()
    {
        var json = """
        {
            "messageType": "A",
            "service": "crypto_data",
            "data": ["X", "btcusd"]
        }
        """;

        Assert.Throws<NotSupportedException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_UnsupportedIexLength_ThrowsNotSupportedException()
    {
        var json = """
        {
            "messageType": "A",
            "service": "iex",
            "data": ["2023-01-15T15:30:00.000000+00:00", "AAPL", 150.75, 200, 300]
        }
        """;

        Assert.Throws<NotSupportedException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_UnsupportedService_ThrowsNotSupportedException()
    {
        var json = """
        {
            "messageType": "A",
            "service": "unknown_service",
            "data": [1, 2, 3]
        }
        """;

        Assert.Throws<NotSupportedException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_InvalidJson_ThrowsJsonException()
    {
        var json = "not a json";

        Assert.Catch<JsonException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    [TestCase("U")]
    [TestCase("D")]
    public void CreateResponseFromJson_UnsupportedMessageTypes_ThrowsNotSupportedException(string msgType)
    {
        var json = $$"""
        {
            "messageType": "{{msgType}}"
        }
        """;

        Assert.Throws<NotSupportedException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_NullTickerInTrade_ThrowsNullReferenceException()
    {
        var json = """
        {
            "messageType": "A",
            "service": "crypto_data",
            "data": ["T", null, "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25]
        }
        """;

        Assert.Throws<NullReferenceException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_NullExchangeInTrade_ThrowsNullReferenceException()
    {
        var json = """
        {
            "messageType": "A",
            "service": "crypto_data",
            "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", null, 0.05, 3450.25]
        }
        """;

        Assert.Throws<NullReferenceException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    // An IEX top-of-book frame whose 'data' precedes 'service'. Its first element is a
    // single character, so shape-based inference classified it as a crypto quote and built
    // a CryptoQuoteUpdate out of IEX fields without raising anything.
    [Test]
    public void CreateResponseFromJson_IexShapedDataBeforeService_ThrowsNotSupportedException()
    {
        var json = """
        {
            "data": ["Q", "AAPL", "2023-01-15T15:30:00.000000+00:00", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13],
            "service": "iex",
            "messageType": "A"
        }
        """;

        Assert.Throws<NotSupportedException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    // A null 'response' used to send the inner loop walking the outer object, swallowing
    // the sibling 'data' property and its subscriptionId along with it.
    [Test]
    public void CreateResponseFromJson_NullResponseObject_ThrowsJsonException()
    {
        var json = """
        {
            "messageType": "I",
            "response": null,
            "data": {
                "subscriptionId": 42
            }
        }
        """;

        Assert.Catch<JsonException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_UtilityWithoutResponse_ThrowsJsonException()
    {
        var json = """
        {
            "messageType": "I",
            "data": {
                "subscriptionId": 42
            }
        }
        """;

        Assert.Catch<JsonException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_UtilityWithoutResponseMessage_ThrowsJsonException()
    {
        var json = """
        {
            "messageType": "H",
            "response": {
                "code": 200
            }
        }
        """;

        Assert.Catch<JsonException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_NonStringService_ThrowsJsonException()
    {
        var json = """
        {
            "messageType": "A",
            "service": null,
            "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25]
        }
        """;

        Assert.Catch<JsonException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_DataResponseWithoutService_ThrowsJsonException()
    {
        var json = """
        {
            "messageType": "A",
            "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25]
        }
        """;

        Assert.Catch<JsonException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    // "\u0041" and "\u0054" are legal JSON spellings of "A" and "T".
    // ValueSpan hands back the raw, still-escaped slice, so reading its first byte yields a
    // backslash. Raw string literals do not process escapes, so these reach the parser as
    // written.
    [Test]
    public void CreateResponseFromJson_EscapedMessageType_ParsesCorrectly()
    {
        var json = """
        {
            "messageType": "\u0041",
            "service": "crypto_data",
            "data": ["\u0054", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25]
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<DataResponse>());
        var dataResponse = (DataResponse)response;
        Assert.That(dataResponse.MessageType, Is.EqualTo('A'));
        Assert.That(dataResponse.Data, Is.InstanceOf<CryptoTradeUpdate>());
        Assert.That(((CryptoTradeUpdate)dataResponse.Data).UpdateMessageType, Is.EqualTo('T'));
    }

    // One element short. Previously surfaced as InvalidOperationException from GetDouble
    // reading straight through the EndArray token.
    [Test]
    public void CreateResponseFromJson_TruncatedForexData_ThrowsJsonException()
    {
        var json = """
        {
            "messageType": "A",
            "service": "fx",
            "data": ["Q", "eurusd", "2023-08-01T10:15:30.500000+00:00", 1000000, 1.0950, 1.0951, 1000000]
        }
        """;

        Assert.Catch<JsonException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    // Trailing elements must stay tolerated: they do not shift the fields ahead of them, and
    // rejecting them would break the library the day Tiingo appends one.
    [Test]
    public void CreateResponseFromJson_TrailingExtraDataElement_ParsesCorrectly()
    {
        var json = """
        {
            "messageType": "A",
            "service": "crypto_data",
            "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25, "future-field"]
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        var trade = (CryptoTradeUpdate)((DataResponse)response).Data;
        Assert.That(trade.Ticker, Is.EqualTo("btcusd"));
        Assert.That(trade.LastPrice, Is.EqualTo(3450.25f).Within(0.0001f));
    }

    // Generalizes the data-before-service fix past 'T': an fx quote and a crypto quote are
    // both 'Q' and differ only in arity, which is exactly what shape inference got wrong.
    [Test]
    public void CreateResponseFromJson_ForexDataBeforeService_ParsesCorrectly()
    {
        var json = """
        {
            "data": ["Q", "eurusd", "2023-08-01T10:15:30.500000+00:00", 1000000, 1.0950, 1.0951, 1000000, 1.0952],
            "service": "fx",
            "messageType": "A"
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        var dataResponse = (DataResponse)response;
        Assert.That(dataResponse.Service, Is.EqualTo("fx"));
        Assert.That(dataResponse.Data, Is.InstanceOf<ForexQuoteUpdate>());
        var fx = (ForexQuoteUpdate)dataResponse.Data;
        Assert.That(fx.Ticker, Is.EqualTo("eurusd"));
        Assert.That(fx.AskPrice, Is.EqualTo(1.0952f).Within(0.0001f));
    }

    [Test]
    public void CreateResponseFromJson_IexDataBeforeService_ParsesCorrectly()
    {
        var json = """
        {
            "data": ["2023-01-15T15:30:00.000000+00:00", "AAPL", 150.75],
            "service": "iex",
            "messageType": "A"
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        var dataResponse = (DataResponse)response;
        Assert.That(dataResponse.Service, Is.EqualTo("iex"));
        Assert.That(dataResponse.Data, Is.InstanceOf<IexReferencePriceUpdate>());
    }

    // Unknown properties, including ones carrying nested containers, must be skipped without
    // disturbing the sibling properties that follow them.
    [Test]
    public void CreateResponseFromJson_UnknownNestedProperties_AreIgnored()
    {
        var json = """
        {
            "meta": { "nested": { "deep": [1, 2, 3] } },
            "messageType": "A",
            "extra": [{ "a": 1 }, [2, 3]],
            "service": "crypto_data",
            "data": ["T", "btcusd", "2019-01-29T19:35:10.923490+00:00", "binance", 0.05, 3450.25],
            "trailing": null
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        var trade = (CryptoTradeUpdate)((DataResponse)response).Data;
        Assert.That(trade.Ticker, Is.EqualTo("btcusd"));
        Assert.That(trade.LastPrice, Is.EqualTo(3450.25f).Within(0.0001f));
    }

    // A WebSocket close frame yields a zero-length payload. That has to fail fast rather
    // than spin, since it reaches the parser from inside the receive loop.
    [Test]
    public void CreateResponseFromJson_EmptyPayload_ThrowsJsonException()
    {
        Assert.Catch<JsonException>(() =>
            ResponseFactory.CreateResponseFromJson(ReadOnlySpan<byte>.Empty));
    }

    [Test]
    public void CreateResponseFromJson_TruncatedPayload_ThrowsJsonException()
    {
        var json = "{\"messageType\": \"A\", \"service\": \"crypto_data\", \"data\": [\"T\", \"btcusd";

        Assert.Catch<JsonException>(() =>
            ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json)));
    }

    [Test]
    public void CreateResponseFromJson_ErrorResponse_ReturnsUtilityResponse()
    {
        var json = """
        {
            "messageType": "E",
            "response": {
                "code": 400,
                "message": "Invalid subscription"
            }
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<UtilityResponse>());
        var utility = (UtilityResponse)response;
        Assert.That(utility.MessageType, Is.EqualTo('E'));
        Assert.That(utility.ResponseCode, Is.EqualTo(400));
        Assert.That(utility.ResponseMessage, Is.EqualTo("Invalid subscription"));
    }

    // Error frames are parsed best-effort so the caller keeps whatever diagnostic arrived
    // instead of trading it for a schema error.
    [Test]
    public void CreateResponseFromJson_MalformedErrorResponse_ReturnsUtilityResponse()
    {
        var json = """
        {
            "messageType": "E"
        }
        """;

        var response = ResponseFactory.CreateResponseFromJson(Encoding.UTF8.GetBytes(json));

        Assert.That(response, Is.InstanceOf<UtilityResponse>());
        Assert.That(response.MessageType, Is.EqualTo('E'));
    }
}

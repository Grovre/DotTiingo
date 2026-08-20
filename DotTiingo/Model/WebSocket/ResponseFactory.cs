using DotTiingo.Model.WebSocket.Response;
using System.Text.Json;

namespace DotTiingo.Model.WebSocket;

internal static class ResponseFactory
{
    private const string ServiceCryptoData = "crypto_data";
    private const string ServiceIex = "iex";
    private const string ServiceFx = "fx";
    private const string ServiceCons = "cons";

    private enum ServiceKind
    {
        None,
        CryptoData,
        Iex,
        Fx,
        Cons,
        Unsupported
    }

    public static AbstractResponse CreateResponseFromJson(ReadOnlySpan<byte> utf8Json)
    {
        var reader = new Utf8JsonReader(utf8Json);
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject");
        }

        var messageType = '\0';
        string? service = null;
        var serviceKind = ServiceKind.None;
        int? responseCode = null;
        string? responseMessage = null;
        int? subscriptionId = null;
        Utf8JsonReader dataArrayReader = default;
        var hasDataArray = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            if (reader.ValueTextEquals("messageType"u8))
            {
                messageType = ReadSingleChar(ref reader, "messageType");
            }
            else if (reader.ValueTextEquals("service"u8))
            {
                ReadNext(ref reader);
                // ValueTextEquals throws on anything that is not a string or a property
                // name, so the token type has to be checked before the comparisons below.
                if (reader.TokenType != JsonTokenType.String)
                    throw new JsonException(
                        $"Expected 'service' to be a string but found {reader.TokenType}.");

                if (reader.ValueTextEquals("crypto_data"u8))
                {
                    service = ServiceCryptoData;
                    serviceKind = ServiceKind.CryptoData;
                }
                else if (reader.ValueTextEquals("iex"u8))
                {
                    service = ServiceIex;
                    serviceKind = ServiceKind.Iex;
                }
                else if (reader.ValueTextEquals("fx"u8))
                {
                    service = ServiceFx;
                    serviceKind = ServiceKind.Fx;
                }
                else if (reader.ValueTextEquals("cons"u8))
                {
                    service = ServiceCons;
                    serviceKind = ServiceKind.Cons;
                }
                else
                {
                    // Only unrecognized services pay for a string allocation, and only so
                    // that the name can be reported in the exception below.
                    service = reader.GetString();
                    serviceKind = ServiceKind.Unsupported;
                }
            }
            else if (reader.ValueTextEquals("response"u8))
            {
                ReadNext(ref reader);
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException(
                        $"Expected 'response' to be an object but found {reader.TokenType}.");

                while (true)
                {
                    ReadNext(ref reader);
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("code"u8))
                    {
                        ReadNext(ref reader);
                        if (reader.TokenType != JsonTokenType.Number)
                            throw new JsonException(
                                $"Expected 'response.code' to be a number but found {reader.TokenType}.");
                        responseCode = reader.GetInt32();
                    }
                    else if (reader.ValueTextEquals("message"u8))
                    {
                        responseMessage = ReadString(ref reader, "response.message");
                    }
                    else
                    {
                        ReadNext(ref reader);
                        reader.Skip();
                    }
                }
            }
            else if (reader.ValueTextEquals("data"u8))
            {
                ReadNext(ref reader);
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    while (true)
                    {
                        ReadNext(ref reader);
                        if (reader.TokenType == JsonTokenType.EndObject)
                            break;
                        if (reader.TokenType != JsonTokenType.PropertyName)
                            continue;

                        if (reader.ValueTextEquals("subscriptionId"u8))
                        {
                            ReadNext(ref reader);
                            if (reader.TokenType != JsonTokenType.Number)
                                throw new JsonException(
                                    $"Expected 'data.subscriptionId' to be a number but found {reader.TokenType}.");
                            subscriptionId = reader.GetInt32();
                        }
                        else
                        {
                            ReadNext(ref reader);
                            reader.Skip();
                        }
                    }
                }
                else if (reader.TokenType == JsonTokenType.StartArray)
                {
                    // Utf8JsonReader is a struct, so this snapshot is a cheap copy that can
                    // be replayed later. Parsing here instead would mean guessing the payload
                    // shape whenever 'data' precedes 'service' in the frame, and guessing
                    // wrong yields a plausible-looking update built from the wrong fields.
                    dataArrayReader = reader;
                    hasDataArray = true;
                    reader.Skip();
                }
            }
            else
            {
                ReadNext(ref reader);
                reader.Skip();
            }
        }

        switch (messageType)
        {
            case 'A': // New data
                if (!hasDataArray)
                    throw new JsonException("Data response was missing its 'data' array.");
                if (service == null)
                    throw new JsonException("Data response was missing its 'service'.");
                return new DataResponse(
                    messageType,
                    service,
                    ParseDataArray(ref dataArrayReader, serviceKind, service));

            case 'I': // Informational/meta data
            case 'H': // Heartbeats
                return new UtilityResponse(
                    messageType,
                    responseCode ?? throw new JsonException(
                        $"Utility response '{messageType}' was missing 'response.code'."),
                    responseMessage ?? throw new JsonException(
                        $"Utility response '{messageType}' was missing 'response.message'."),
                    messageType == 'I' ? subscriptionId : null);

            case 'E': // Error messages
                // Deliberately lenient: the caller turns this into an exception carrying the
                // raw frame, so a malformed error frame must not be masked by a schema error
                // that discards whatever diagnostic Tiingo did manage to send.
                return new UtilityResponse(
                    messageType,
                    responseCode ?? 0,
                    responseMessage ?? string.Empty,
                    subscriptionId);

            // 'U' (update) and 'D' (delete) are part of Tiingo's documented message set but
            // are not emitted by any service this library supports, so they land here with
            // everything else that is unrecognized.
            default:
                throw new NotSupportedException(
                    $"Message type '{messageType}' not supported.");
        }
    }

    /// <summary>
    /// Parses a 'data' array. The reader must be positioned at its StartArray token.
    /// Elements past the ones a shape needs are ignored, so an appended field does not
    /// break the shapes that already work.
    /// </summary>
    private static IResponseData ParseDataArray(
        ref Utf8JsonReader reader,
        ServiceKind serviceKind,
        string service)
    {
        switch (serviceKind)
        {
            case ServiceKind.CryptoData:
                return ParseCryptoDataArray(ref reader);
            case ServiceKind.Iex:
                return ParseIexDataArray(ref reader);
            case ServiceKind.Fx:
                return ParseFxDataArray(ref reader);
            case ServiceKind.Cons:
                return ParseConsDataArray(ref reader);
            default:
                throw new NotSupportedException(
                    $"Service '{service}' not supported.");
        }
    }

    private static IResponseData ParseCryptoDataArray(ref Utf8JsonReader reader)
    {
        var updateMessageType = ReadSingleChar(ref reader, "data[0]");
        switch (updateMessageType)
        {
            case 'T':
            {
                var ticker = ReadString(ref reader, "ticker");
                var dttm = ReadDateTimeOffset(ref reader, "dttm");
                var exchange = ReadString(ref reader, "exchange");
                var lastSize = ReadSingle(ref reader, "lastSize");
                var lastPrice = ReadSingle(ref reader, "lastPrice");
                return new CryptoTradeUpdate(
                    updateMessageType,
                    ticker,
                    dttm,
                    exchange,
                    lastSize,
                    lastPrice);
            }
            case 'Q':
            {
                var ticker = ReadString(ref reader, "ticker");
                var dttm = ReadDateTimeOffset(ref reader, "dttm");
                var exchange = ReadString(ref reader, "exchange");
                var bidSize = ReadSingle(ref reader, "bidSize");
                var bidPrice = ReadSingle(ref reader, "bidPrice");
                var midPrice = ReadSingle(ref reader, "midPrice");
                var askSize = ReadSingle(ref reader, "askSize");
                var askPrice = ReadSingle(ref reader, "askPrice");
                return new CryptoQuoteUpdate(
                    updateMessageType,
                    ticker,
                    dttm,
                    exchange,
                    bidSize,
                    bidPrice,
                    midPrice,
                    askSize,
                    askPrice);
            }
            default:
                throw new NotSupportedException(
                    $"Data message type '{updateMessageType}' not supported.");
        }
    }

    private static IResponseData ParseIexDataArray(ref Utf8JsonReader reader)
    {
        // Kept so the array length can be reported if the shape is not recognized.
        var arrayStart = reader;

        // The only supported IEX shape is the 3-element reference price update:
        // [timestamp, ticker, referencePrice]. Every other shape is reported by its length,
        // so this probe tests token types rather than letting a field-level failure escape
        // and mask the length.
        if (reader.Read()
            && reader.TokenType == JsonTokenType.String
            && reader.TryGetDateTimeOffset(out var dttm)
            && reader.Read()
            && reader.TokenType == JsonTokenType.String)
        {
            var ticker = reader.GetString()!;
            if (reader.Read() && reader.TokenType == JsonTokenType.Number)
            {
                var referencePrice = reader.GetSingle();
                if (reader.Read() && reader.TokenType == JsonTokenType.EndArray)
                    return new IexReferencePriceUpdate(dttm, ticker, referencePrice);
            }
        }

        throw new NotSupportedException(
            $"IEX message with array length '{CountArrayElements(ref arrayStart)}' not supported.");
    }

    private static IResponseData ParseFxDataArray(ref Utf8JsonReader reader)
    {
        var updateMessageType = ReadSingleChar(ref reader, "data[0]");
        var ticker = ReadString(ref reader, "ticker");
        var dttm = ReadDateTimeOffset(ref reader, "dttm");
        var bidSize = ReadSingle(ref reader, "bidSize");
        var bidPrice = ReadSingle(ref reader, "bidPrice");
        var midPrice = ReadSingle(ref reader, "midPrice");
        var askSize = ReadSingle(ref reader, "askSize");
        var askPrice = ReadSingle(ref reader, "askPrice");
        return new ForexQuoteUpdate(
            updateMessageType,
            ticker,
            dttm,
            bidSize,
            bidPrice,
            midPrice,
            askSize,
            askPrice);
    }

#pragma warning disable TNGOBETA
    private static IResponseData ParseConsDataArray(ref Utf8JsonReader reader)
    {
        var arrayStart = reader;
        var count = CountArrayElements(ref arrayStart);

        if (count == 3)
        {
            var dttm = ReadDateTimeOffset(ref reader, "date");
            var ticker = ReadString(ref reader, "ticker");
            var referencePrice = ReadSingle(ref reader, "referencePrice");
            return new EquityRealtimeReferencePriceUpdate(dttm, ticker, referencePrice);
        }

        if (count >= 8)
        {
            var dttm = ReadDateTimeOffset(ref reader, "date");
            var ticker = ReadString(ref reader, "ticker");
            var lqSpread = ReadSingle(ref reader, "lqSpread");
            var lqBidSize = ReadInt32(ref reader, "lqBidSize");
            var lqBidPrice = ReadSingle(ref reader, "lqBidPrice");
            var referencePrice = ReadSingle(ref reader, "referencePrice");
            var lqAskPrice = ReadSingle(ref reader, "lqAskPrice");
            var lqAskSize = ReadInt32(ref reader, "lqAskSize");
            return new EquityRealtimeLiquidityRiskMetricUpdate(
                dttm,
                ticker,
                lqSpread,
                lqBidSize,
                lqBidPrice,
                referencePrice,
                lqAskPrice,
                lqAskSize);
        }

        throw new NotSupportedException(
            $"Consolidated equity message with array length '{count}' not supported.");
    }
#pragma warning restore TNGOBETA

    /// <summary>
    /// Counts the elements of the array the reader is positioned at (its StartArray token).
    /// </summary>
    private static int CountArrayElements(ref Utf8JsonReader reader)
    {
        var count = 0;
        while (true)
        {
            ReadNext(ref reader);
            if (reader.TokenType == JsonTokenType.EndArray)
                return count;
            if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                reader.Skip();
            count++;
        }
    }

    /// <summary>
    /// Advances the reader, failing loudly rather than leaving a caller to spin on an
    /// unchanged token.
    /// </summary>
    private static void ReadNext(ref Utf8JsonReader reader)
    {
        if (!reader.Read())
            throw new JsonException("Unexpected end of JSON payload.");
    }

    private static char ReadSingleChar(ref Utf8JsonReader reader, string name)
    {
        ReadNext(ref reader);
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException(
                $"Expected '{name}' to be a string but found {reader.TokenType}.");

        // Fast path: an unescaped single ASCII byte, which is what Tiingo sends.
        if (!reader.ValueIsEscaped)
        {
            var span = reader.ValueSpan;
            if (span.Length == 1 && span[0] <= 0x7F)
                return (char)span[0];
        }

        // Slow path. ValueSpan is the raw JSON slice, neither unescaped nor decoded, so a
        // JSON escape sequence or a multi-byte character has to go through GetString.
        var value = reader.GetString();
        if (value is not { Length: 1 })
            throw new JsonException($"Expected '{name}' to be a single-character string.");
        return value[0];
    }

    private static string ReadString(ref Utf8JsonReader reader, string name)
    {
        ReadNext(ref reader);
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString()!,
            JsonTokenType.Null => throw new NullReferenceException(
                $"Deserialized json variable '{name}' was null"),
            _ => throw new JsonException(
                $"Expected '{name}' to be a string but found {reader.TokenType}.")
        };
    }

    private static float ReadSingle(ref Utf8JsonReader reader, string name)
    {
        ReadNext(ref reader);
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException(
                $"Expected '{name}' to be a number but found {reader.TokenType}.");
        return reader.GetSingle();
    }

    private static int ReadInt32(ref Utf8JsonReader reader, string name)
    {
        ReadNext(ref reader);
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException(
                $"Expected '{name}' to be a number but found {reader.TokenType}.");
        return reader.GetInt32();
    }

    private static DateTimeOffset ReadDateTimeOffset(ref Utf8JsonReader reader, string name)
    {
        ReadNext(ref reader);
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException(
                $"Expected '{name}' to be a date string but found {reader.TokenType}.");
        if (!reader.TryGetDateTimeOffset(out var value))
            throw new JsonException($"Could not parse '{name}' as a date.");
        return value;
    }
}

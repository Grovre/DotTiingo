using DotTiingo.Model.WebSocket.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotTiingo.Model.WebSocket;

internal class ResponseFactory
{
    public AbstractResponse CreateResponseFromJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        var jsonElement = document.RootElement;
        var messageType = jsonElement.GetProperty("messageType").GetString()[0];
        AbstractResponse response;
        IResponseData data;

        switch (messageType)
        {
            case 'A': // New data
                var service = jsonElement.GetProperty("service").GetString();
                string ticker;
                DateTime dttm;
                string exchange;
                char updateMessageType;
                float bidSize;
                float bidPrice;
                float midPrice;
                float askSize;
                float askPrice;
                float lastSize;
                float lastPrice;
                switch (service)
                {
                    case "crypto_data":
                        jsonElement = jsonElement.GetProperty("data");
                        updateMessageType = jsonElement[0].GetString()[0];
                        switch (updateMessageType)
                        {
                            case 'T':
                                ticker = jsonElement[1].GetString();
                                dttm = jsonElement[2].GetDateTime();
                                exchange = jsonElement[3].GetString();
                                lastSize = (float)jsonElement[4].GetDouble();
                                lastPrice = (float)jsonElement[5].GetDouble();
                                data = new CryptoTradeUpdate(
                                    updateMessageType,
                                    ticker,
                                    dttm,
                                    exchange,
                                    lastSize,
                                    lastPrice);
                                response = new DataResponse(
                                    messageType,
                                    service,
                                    data);
                                break;
                            case 'Q':
                                ticker = jsonElement[1].GetString();
                                dttm = jsonElement[2].GetDateTime();
                                exchange = jsonElement[3].GetString();
                                bidSize = (float)jsonElement[4].GetDouble();
                                bidPrice = (float)jsonElement[5].GetDouble();
                                midPrice = (float)jsonElement[6].GetDouble();
                                askSize = (float)jsonElement[7].GetDouble();
                                askPrice = (float)jsonElement[8].GetDouble();
                                data = new CryptoQuoteUpdate(
                                    updateMessageType,
                                    ticker,
                                    dttm,
                                    exchange,
                                    bidSize,
                                    bidPrice,
                                    midPrice,
                                    askSize,
                                    askPrice);
                                response = new DataResponse(
                                    messageType, 
                                    service, 
                                    data);
                                break;
                            default:
                                throw new NotSupportedException(
                                    $"Data message type '{updateMessageType}' not supported.");
                        }
                        break;
                    case "iex":
                        jsonElement = jsonElement.GetProperty("data");
                        var arrLen = jsonElement.GetArrayLength();
                        switch (arrLen)
                        {
                            case 3:
                                dttm = jsonElement[0].GetDateTime();
                                ticker = jsonElement[1].GetString();
                                lastPrice = (float)jsonElement[2].GetDouble(); // Ref price
                                data = new IexReferencePriceUpdate(
                                    dttm,
                                    ticker,
                                    lastPrice);
                                response = new DataResponse(
                                    messageType,
                                    service,
                                    data);
                                break;
                            case 16:
                            default:
                                throw new NotSupportedException(
                                    $"IEX message with array length '{arrLen}' not supported.");
                        }
                        break;
                    case "fx":
                        jsonElement = jsonElement.GetProperty("data");
                        updateMessageType = jsonElement[0].ToString()[0];
                        ticker = jsonElement[1].GetString();
                        dttm = jsonElement[2].GetDateTime();
                        bidSize = (float)jsonElement[3].GetDouble();
                        bidPrice = (float)jsonElement[4].GetDouble();
                        midPrice = (float)jsonElement[5].GetDouble();
                        askSize = (float)jsonElement[6].GetDouble();
                        askPrice = (float)jsonElement[7].GetDouble();
                        data = new ForexQuoteUpdate(
                            updateMessageType,
                            ticker,
                            dttm,
                            bidSize,
                            bidPrice,
                            midPrice,
                            askSize,
                            askPrice);
                        response = new DataResponse(
                            messageType,
                            service,
                            data);
                        break;
                    default:
                        throw new NotSupportedException(
                            $"Service '{service}' not supported.");
                }
                break;
            case 'U': // Updating existing data
                goto default;
            case 'D': // Deleting existing data
                goto default;
            case 'I': // Informational/meta data
                var responseElement = jsonElement.GetProperty("response");
                var responseCode = responseElement.GetProperty("code").GetInt32();
                var responseMessage = responseElement.GetProperty("message").GetString();
                var subId = jsonElement.GetProperty("data").GetProperty("subscriptionId").GetInt32();
                response = new UtilityResponse(
                    messageType,
                    responseCode,
                    responseMessage,
                    subId);
                break;
            case 'E': // Error messages
                goto default;
            case 'H': // Heartbeats
                responseElement = jsonElement.GetProperty("response");
                responseCode = responseElement.GetProperty("code").GetInt32();
                responseMessage = responseElement.GetProperty("message").GetString();
                response = new UtilityResponse(
                    messageType,
                    responseCode,
                    responseMessage,
                    null);
                break;
            default:
                throw new NotSupportedException(
                    $"Message type '{messageType}' not supported.");
        }

        return response;
    }
}

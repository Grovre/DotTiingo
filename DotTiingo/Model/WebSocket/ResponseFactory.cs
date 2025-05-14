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
        var responseMsgType = jsonElement.GetProperty("messageType").GetString()[0];
        AbstractResponse response;
        IResponseData data;

        switch (responseMsgType)
        {
            case 'A': // New data
                var service = jsonElement.GetProperty("service").GetString();
                switch (service)
                {
                    case "crypto_data":
                        jsonElement = jsonElement.GetProperty("data");
                        var dataMsgType = jsonElement[0].GetString()[0];
                        switch (dataMsgType)
                        {
                            case 'T':
                                var ticker = jsonElement[1].GetString();
                                var dttm = jsonElement[2].GetDateTime();
                                var exchange = jsonElement[3].GetString();
                                var lastSize = jsonElement[4].GetDouble();
                                var lastPrice = jsonElement[5].GetDouble();
                                data = new CryptoTradeUpdate(
                                    dataMsgType,
                                    ticker,
                                    dttm,
                                    exchange,
                                    (float)lastSize,
                                    (float)lastPrice);
                                response = new DataResponse(
                                    responseMsgType,
                                    service,
                                    data);
                                break;
                            case 'Q':
                                ticker = jsonElement[1].GetString();
                                dttm = jsonElement[2].GetDateTime();
                                exchange = jsonElement[3].GetString();
                                var bidSize = jsonElement[4].GetDouble();
                                var bidPrice = jsonElement[5].GetDouble();
                                var midPrice = jsonElement[6].GetDouble();
                                var askSize = jsonElement[7].GetDouble();
                                var askPrice = jsonElement[8].GetDouble();
                                data = new CryptoQuoteUpdate(
                                    dataMsgType,
                                    ticker,
                                    dttm,
                                    exchange,
                                    (float)bidSize,
                                    (float)bidPrice,
                                    (float)midPrice,
                                    (float)askSize,
                                    (float)askPrice);
                                response = new DataResponse(
                                    responseMsgType, 
                                    service, 
                                    data);
                                break;
                            default:
                                throw new NotSupportedException(
                                    $"Data message type '{dataMsgType}' not supported.");
                        }
                        break;
                    case "iex":
                    case "fx":
                        throw new NotSupportedException(
                            $"Service '{service}' not supported.");
                    default:
                        throw new NotSupportedException(
                            $"Service '{service}' not supported.");
                }
                break;
            case 'U': // Updating existing data
                throw new NotSupportedException(
                    $"Message type '{responseMsgType}' not supported.");
            case 'D': // Deleting existing data
                throw new NotSupportedException(
                    $"Message type '{responseMsgType}' not supported.");
            case 'I': // Informational/meta data
                var responseElement = jsonElement.GetProperty("response");
                var responseCode = responseElement.GetProperty("code").GetInt32();
                var responseMessage = responseElement.GetProperty("message").GetString();
                var subId = jsonElement.GetProperty("data").GetProperty("subscriptionId").GetInt32();
                response = new UtilityResponse(
                    responseMsgType,
                    responseCode,
                    responseMessage,
                    subId);
                break;
            case 'E': // Error messages
                throw new NotSupportedException(
                    $"Error message type '{responseMsgType}' not supported.");
            case 'H': // Heartbeats
                responseElement = jsonElement.GetProperty("response");
                responseCode = responseElement.GetProperty("code").GetInt32();
                responseMessage = responseElement.GetProperty("message").GetString();
                response = new UtilityResponse(
                    responseMsgType,
                    responseCode,
                    responseMessage,
                    null);
                break;
            default:
                throw new NotSupportedException(
                    $"Message type '{responseMsgType}' not supported.");
        }

        return response;
    }
}

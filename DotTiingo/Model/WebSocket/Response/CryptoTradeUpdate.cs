using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.WebSocket.Response;

public record CryptoTradeUpdate(
    char UpdateMessageType,
    string Ticker,
    DateTime Date,
    string Exchange,
    float LastSize,
    float LastPrice) : IResponseData;

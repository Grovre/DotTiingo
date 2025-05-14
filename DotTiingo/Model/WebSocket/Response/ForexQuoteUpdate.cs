using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.WebSocket.Response;

public record ForexQuoteUpdate(
    char UpdateMessageType,
    string Ticker,
    DateTime Date,
    float BidSize,
    float BidPrice,
    float MidPrice,
    float AskSize,
    float AskPrice) : IResponseData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.WebSocket.Response;

public record CryptoQuoteUpdate(
    char UpdateMessageType,
    string Ticker,
    DateTime Date,
    string Exchange,
    float BidSize,
    float BidPrice,
    float MidPrice,
    float AskSize,
    float AskPrice) : IResponseData;

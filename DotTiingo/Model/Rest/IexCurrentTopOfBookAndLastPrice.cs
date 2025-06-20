using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.Rest;

public record IexCurrentTopOfBookAndLastPrice(
    string Ticker,
    DateTimeOffset Timestamp,
    DateTimeOffset? QuoteTimestamp,
    DateTimeOffset? LastSaleTimestamp,
    float? Last,
    int? LastSize,
    float TngoLast,
    float PreviousClose,
    float Open,
    float High,
    float Low,
    float? Mid,
    long Volume,
    float? BidSize,
    float? BidPrice,
    float? AskSize,
    float? AskPrice);
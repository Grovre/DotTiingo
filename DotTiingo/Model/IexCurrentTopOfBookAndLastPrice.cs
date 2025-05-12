using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model;

public record IexCurrentTopOfBookAndLastPrice(
    string Ticker,
    DateTime Timestamp,
    DateTime? QuoteTimestamp,
    DateTime? LastSaleTimestamp,
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
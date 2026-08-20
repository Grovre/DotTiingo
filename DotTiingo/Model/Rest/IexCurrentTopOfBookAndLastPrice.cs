using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    float PrevClose,
    float Open,
    float High,
    float Low,
    float? Mid,
    [property: JsonConverter(typeof(LenientInt64Converter))]
    long Volume,
    float? BidSize,
    float? BidPrice,
    float? AskSize,
    float? AskPrice);

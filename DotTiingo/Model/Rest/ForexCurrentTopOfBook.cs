namespace DotTiingo.Model.Rest;

public record ForexCurrentTopOfBook(
    string Ticker,
    DateTimeOffset QuoteTimestamp,
    float MidPrice,
    float BidSize,
    float BidPrice,
    float AskSize,
    float AskPrice);
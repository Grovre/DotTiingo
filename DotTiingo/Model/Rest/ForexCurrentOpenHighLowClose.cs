namespace DotTiingo.Model.Rest;

public record ForexCurrentOpenHighLowClose(
    DateTimeOffset Date,
    string Ticker,
    float Open,
    float High,
    float Low,
    float Close);
namespace DotTiingo.Model.Rest;

public record CryptoPrice(
    string Ticker,
    string BaseCurrency,
    string QuoteCurrency,
    PriceData[] PriceData);

namespace DotTiingo.Model;

public record CryptoPrice(
    string Ticker,
    string BaseCurrency,
    string QuoteCurrency,
    PriceData[] PriceData);

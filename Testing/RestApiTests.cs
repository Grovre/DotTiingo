using DotTiingo;
using DotTiingo.Model.Rest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing;

[TestFixture]
public class RestApiTests
{
    private HttpClient _httpClient = null!;
    private TiingoClient _client = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        _httpClient = new HttpClient();
        _client = new TiingoClient(_httpClient, TestConfiguration.TiingoToken);
    }

    [Test]
    public async Task EndOfDayPrices()
    {
        var prices = await _client.Rest.EndOfDay.GetEndOfDayPrices("AAPL", new(DateTime.UtcNow - TimeSpan.FromDays(90), DateTime.UtcNow), "daily", "volume");
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices, Has.Length.Positive);
        Assert.That(prices, Is.EquivalentTo(prices.OrderBy(x => x.Volume)));
    }

    [Test]
    public async Task EndOfDayMeta()
    {
        var meta = await _client.Rest.EndOfDay.GetEndOfDayMeta("AAPL");
        Assert.That(meta, Is.Not.Null);
        Assert.That(meta.Ticker, Is.EqualTo("AAPL"));
        Assert.That(meta.Name, Is.EqualTo("Apple Inc"));
        Assert.That(meta.ExchangeCode, Is.EqualTo("NASDAQ"));
    }

    [Test]
    public async Task News()
    {
        var news = await _client.Rest.News.GetNews(["AAPL", "NVDA", "INTC"], null, new(DateTime.UtcNow - TimeSpan.FromDays(90), DateTime.UtcNow), 1, null, "publishedDate");
        Assert.That(news, Is.Not.Null);
        Assert.That(news, Has.Length.Positive);
        Assert.That(news, Is.EquivalentTo(news.OrderBy(x => x.PublishedDate)));
    }

    [Test]
    public async Task CryptoPrices()
    {
        var prices = await _client.Rest.Crypto.GetCryptoPrices(["btcusd"], null, new(DateTime.UtcNow - TimeSpan.FromDays(90), DateTime.UtcNow), null);
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices, Has.Length.Positive);
    }

    [Test]
    public async Task CryptoMeta()
    {
        var meta = await _client.Rest.Crypto.GetCryptoMeta(["btcusd"]);
        Assert.That(meta, Is.Not.Null);
        Assert.That(meta, Has.Length.Positive);
    }

    [Test]
    public async Task IexCurrentTopOfBookAndLastPrice()
    {
        var prices = await _client.Rest.Iex.GetIexCurrentTopOfBookAndLastPrice(["AAPL"]);
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices, Has.Length.Positive);
    }

    [Test]
    public async Task IexHistoricalPrices()
    {
        var interval = new DateTimeInterval(DateTime.UtcNow - TimeSpan.FromDays(90), DateTime.UtcNow);
        var prices = await _client.Rest.Iex.GetIexHistoricalPrices("AAPL", interval, null, null, null);
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices, Has.Length.Positive);
    }
}

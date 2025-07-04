﻿using DotTiingo;
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
        var prices = await _client.Rest.EndOfDay.GetEndOfDayPrices("AAPL", new(DateTimeOffset.UtcNow - TimeSpan.FromDays(90), DateTimeOffset.UtcNow), "daily", "volume");
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
        var news = await _client.Rest.News.GetNews(["AAPL", "NVDA", "INTC"], null, new(DateTimeOffset.UtcNow - TimeSpan.FromDays(90), DateTimeOffset.UtcNow), 5, null, "publishedDate");
        Assert.That(news, Is.Not.Null);
        Assert.That(news, Has.Length.Positive);
        Assert.That(news, Is.EquivalentTo(news.OrderByDescending(x => x.PublishedDate)));
    }

    [Test]
    public async Task CryptoPrices()
    {
        var prices = await _client.Rest.Crypto.GetCryptoPrices(["btcusd"], null, new(DateTimeOffset.UtcNow - TimeSpan.FromDays(90), DateTimeOffset.UtcNow), null);
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
    [TestCase("KOLD")]
    [TestCase("BOIL")]
    [TestCase("AAPL")]
    [TestCase("GOOG")]
    [TestCase("MSFT")]
    public async Task IexHistoricalPrices(string ticker)
    {
        await AssertIexHistoricalPricesValid(ticker);
    }

    private async Task AssertIexHistoricalPricesValid(string ticker)
    {
        var prices = await _client.Rest.Iex.GetIexHistoricalPrices(ticker, null, null, null, null);
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices, Has.Length.Positive);
        foreach (var price in prices)
        {
            Assert.That(price, Is.Not.Null);
            Assert.That(price.Date, Is.Not.EqualTo(default(DateTimeOffset)), "Date should not be default");
            Assert.That(price.Open, Is.Not.EqualTo(default(float)), "Open price should not be default");
            Assert.That(price.High, Is.Not.EqualTo(default(float)), "High price should not be default");
            Assert.That(price.Low, Is.Not.EqualTo(default(float)), "Low price should not be default");
            Assert.That(price.Close, Is.Not.EqualTo(default(float)), "Close price should not be default");
            if (price.IexVolume == 0)
                Assert.Warn($"Volume is zero for {ticker} on {price.Date.ToLocalTime():d} at {price.Date.ToLocalTime():T}. This may indicate no trading activity.");
        }
    }
}

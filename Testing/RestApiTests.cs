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
        foreach (var price in prices)
        {
            Assert.That(price, Is.Not.Null);
            Assert.That(price.Ticker, Is.EqualTo("AAPL"));
            Assert.That(price.Timestamp, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(price.PrevClose, Is.GreaterThan(0));
        }
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

    [Test]
    public async Task ForexCurrentTopOfBook()
    {
        var prices = await _client.Rest.Forex.GetCurrentTopOfBook(["eurusd", "gbpusd"]);
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices, Has.Length.Positive);
        foreach (var price in prices)
        {
            Assert.That(price, Is.Not.Null);
            Assert.That(price.Ticker, Is.Not.Null.And.Not.Empty);
            Assert.That(price.QuoteTimestamp, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(price.MidPrice, Is.GreaterThan(0));
            Assert.That(price.BidPrice, Is.GreaterThan(0));
            Assert.That(price.AskPrice, Is.GreaterThan(0));
        }
    }

    [Test]
    public async Task ForexOpenHighLowCloseDefault()
    {
        var prices = await _client.Rest.Forex.GetOpenHighLowClose("eurusd");
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices, Has.Length.Positive);
        foreach (var price in prices)
        {
            Assert.That(price, Is.Not.Null);
            Assert.That(price.Ticker, Is.EqualTo("eurusd").IgnoreCase);
            Assert.That(price.Date, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(price.Open, Is.GreaterThan(0));
            Assert.That(price.High, Is.GreaterThan(0));
            Assert.That(price.Low, Is.GreaterThan(0));
            Assert.That(price.Close, Is.GreaterThan(0));
        }
    }

    [Test]
    [TestCase("eurusd")]
    [TestCase("gbpusd")]
    [TestCase("usdjpy")]
    public async Task ForexOpenHighLowClose(string ticker)
    {
        var interval = new DateTimeInterval(DateTimeOffset.UtcNow - TimeSpan.FromDays(7), DateTimeOffset.UtcNow);
        var prices = await _client.Rest.Forex.GetOpenHighLowClose(ticker, "1hour", interval);
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices, Has.Length.Positive);
        foreach (var price in prices)
        {
            Assert.That(price, Is.Not.Null);
            Assert.That(price.Ticker, Is.EqualTo(ticker).IgnoreCase);
            Assert.That(price.Date, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(price.Open, Is.GreaterThan(0));
            Assert.That(price.High, Is.GreaterThan(0));
            Assert.That(price.Low, Is.GreaterThan(0));
            Assert.That(price.Close, Is.GreaterThan(0));
        }
    }

    [Test]
    public async Task EquityRealtimeCurrentReferencePriceAndLiquiditySingleTicker()
    {
        var snapshots = await _client.Rest.EquityRealtime.GetCurrentReferencePriceAndLiquidity("AAPL");
        Assert.That(snapshots, Is.Not.Null);
        Assert.That(snapshots, Has.Length.Positive);
        var snapshot = snapshots[0];
        Assert.That(snapshot.Ticker, Is.EqualTo("AAPL").IgnoreCase);
        Assert.That(snapshot.Timestamp, Is.Not.EqualTo(default(DateTimeOffset)));
        Assert.That(snapshot.PrevClose, Is.GreaterThan(0));
        Assert.That(snapshot.TngoLast, Is.GreaterThan(0));
    }

    [Test]
    public async Task EquityRealtimeCurrentReferencePriceAndLiquidityMultipleTickers()
    {
        var snapshots = await _client.Rest.EquityRealtime.GetCurrentReferencePriceAndLiquidity(["AAPL", "MSFT"]);
        Assert.That(snapshots, Is.Not.Null);
        Assert.That(snapshots, Has.Length.EqualTo(2));
        foreach (var snapshot in snapshots)
        {
            Assert.That(snapshot, Is.Not.Null);
            Assert.That(snapshot.Ticker, Is.Not.Null.And.Not.Empty);
            Assert.That(snapshot.Timestamp, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(snapshot.PrevClose, Is.GreaterThan(0));
        }
    }

    [Test]
    public async Task EquityRealtimeCurrentReferencePriceAndLiquidityAllTickers()
    {
        var snapshots = await _client.Rest.EquityRealtime.GetCurrentReferencePriceAndLiquidity();
        Assert.That(snapshots, Is.Not.Null);
        Assert.That(snapshots, Has.Length.Positive);
        var first = snapshots[0];
        Assert.That(first.Ticker, Is.Not.Null.And.Not.Empty);
        Assert.That(first.Timestamp, Is.Not.EqualTo(default(DateTimeOffset)));
    }

    [Test]
    [TestCase("AAPL")]
    [TestCase("MSFT")]
    [TestCase("GOOG")]
    public async Task EquityHistoricalPrices(string ticker)
    {
        var prices = await _client.Rest.EquityRealtime.GetHistoricalPrices(ticker);
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices, Has.Length.Positive);
        foreach (var price in prices)
        {
            Assert.That(price, Is.Not.Null);
            Assert.That(price.Date, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(price.Open, Is.Not.EqualTo(default(float)));
            Assert.That(price.High, Is.Not.EqualTo(default(float)));
            Assert.That(price.Low, Is.Not.EqualTo(default(float)));
            Assert.That(price.Close, Is.Not.EqualTo(default(float)));
        }
    }

    [Test]
    public async Task EquityHistoricalPricesWithOptions()
    {
        var interval = new DateTimeInterval(DateTimeOffset.UtcNow - TimeSpan.FromDays(7), DateTimeOffset.UtcNow);
        var prices = await _client.Rest.EquityRealtime.GetHistoricalPrices("AAPL", interval, "1hour", true, true);
        Assert.That(prices, Is.Not.Null);
        Assert.That(prices, Has.Length.Positive);
        foreach (var price in prices)
        {
            Assert.That(price, Is.Not.Null);
            Assert.That(price.Date, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(price.Open, Is.GreaterThan(0));
            Assert.That(price.High, Is.GreaterThan(0));
            Assert.That(price.Low, Is.GreaterThan(0));
            Assert.That(price.Close, Is.GreaterThan(0));
        }
    }

    [Test]
    public void EquityRealtimeSnapshotDeserialization()
    {
        var json = """
        [
          {
            "ticker": "AAPL",
            "timestamp": "2026-08-20T11:52:34.604927269-04:00",
            "open": 315.945,
            "high": 320.28,
            "low": 315.27,
            "tngoLast": 316.15,
            "prevClose": 315.945,
            "volume": 531548.0,
            "lqSpread": 0.000032,
            "lqBidPrice": 316.145,
            "lqBidSize": 40,
            "lqRefPrice": 316.15,
            "lqAskPrice": 316.155,
            "lqAskSize": 80
          },
          {
            "ticker": "000425",
            "timestamp": "2026-07-29T20:00:00+00:00",
            "open": 8.76,
            "high": 9.09,
            "low": 8.72,
            "tngoLast": 9.0,
            "volume": 147928660,
            "prevClose": 8.76,
            "lqRefPrice": 9.0,
            "lqSpread": null,
            "lqBidPrice": null,
            "lqBidSize": null,
            "lqAskPrice": null,
            "lqAskSize": null
          }
        ]
        """;

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = System.Text.Json.JsonSerializer.Deserialize<EquityRealtimeSnapshot[]>(json, options);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Length.EqualTo(2));

        var first = result![0];
        Assert.That(first.Ticker, Is.EqualTo("AAPL"));
        Assert.That(first.Open, Is.EqualTo(315.945f));
        Assert.That(first.High, Is.EqualTo(320.28f));
        Assert.That(first.Low, Is.EqualTo(315.27f));
        Assert.That(first.TngoLast, Is.EqualTo(316.15f));
        Assert.That(first.PrevClose, Is.EqualTo(315.945f));
        Assert.That(first.Volume, Is.EqualTo(531548L));
        Assert.That(first.LqSpread, Is.EqualTo(0.000032f));
        Assert.That(first.LqBidPrice, Is.EqualTo(316.145f));
        Assert.That(first.LqBidSize, Is.EqualTo(40L));
        Assert.That(first.LqRefPrice, Is.EqualTo(316.15f));
        Assert.That(first.LqAskPrice, Is.EqualTo(316.155f));
        Assert.That(first.LqAskSize, Is.EqualTo(80L));

        var second = result[1];
        Assert.That(second.Ticker, Is.EqualTo("000425"));
        Assert.That(second.Volume, Is.EqualTo(147928660L));
        Assert.That(second.LqSpread, Is.Null);
        Assert.That(second.LqBidPrice, Is.Null);
        Assert.That(second.LqBidSize, Is.Null);
        Assert.That(second.LqAskPrice, Is.Null);
        Assert.That(second.LqAskSize, Is.Null);
    }

    [Test]
    public void EquityHistoricalPriceDeserialization()
    {
        var json = """
        [
          {
            "date": "2024-01-02T13:00:00.000Z",
            "open": 188.35,
            "high": 188.36,
            "low": 187.89,
            "close": 188.19,
            "volume": 839.0
          },
          {
            "date": "2024-01-02T21:00:00.000Z",
            "open": 185.545,
            "high": 185.545,
            "low": 185.545,
            "close": 185.545,
            "volume": 0.0
          },
          {
            "date": "2024-01-03T00:00:00.000Z",
            "open": 185.0,
            "high": 186.0,
            "low": 184.0,
            "close": 185.5
          }
        ]
        """;

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = System.Text.Json.JsonSerializer.Deserialize<EquityHistoricalPrice[]>(json, options);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Length.EqualTo(3));

        Assert.That(result![0].Open, Is.EqualTo(188.35f));
        Assert.That(result[0].Close, Is.EqualTo(188.19f));
        Assert.That(result[0].Volume, Is.EqualTo(839L));

        Assert.That(result[1].Volume, Is.EqualTo(0L));

        Assert.That(result[2].Volume, Is.Null);
    }

    [Test]
    public async Task FundamentalDefinitionsSingleTicker()
    {
        var definitions = await _client.Rest.Fundamentals.GetDefinitions("AAPL");
        Assert.That(definitions, Is.Not.Null);
        Assert.That(definitions, Has.Length.Positive);
        var first = definitions[0];
        Assert.That(first.DataCode, Is.Not.Null.And.Not.Empty);
        Assert.That(first.Name, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task FundamentalDefinitionsMultipleTickers()
    {
        var definitions = await _client.Rest.Fundamentals.GetDefinitions(["AAPL", "MSFT"]);
        Assert.That(definitions, Is.Not.Null);
        Assert.That(definitions, Has.Length.Positive);
    }

    [Test]
    public async Task FundamentalDefinitionsAllTickers()
    {
        var definitions = await _client.Rest.Fundamentals.GetDefinitions();
        Assert.That(definitions, Is.Not.Null);
        Assert.That(definitions, Has.Length.Positive);
    }

    [Test]
    public async Task FundamentalMetaSingleTicker()
    {
        var metaList = await _client.Rest.Fundamentals.GetMeta("AAPL");
        Assert.That(metaList, Is.Not.Null);
        Assert.That(metaList, Has.Length.Positive);
        var meta = metaList[0];
        Assert.That(meta.Ticker, Is.EqualTo("aapl").IgnoreCase);
        Assert.That(meta.Name, Is.EqualTo("Apple Inc"));
        Assert.That(meta.PermaTicker, Is.Not.Null.And.Not.Empty);
        Assert.That(meta.IsActive, Is.True);
    }

    [Test]
    public async Task FundamentalMetaMultipleTickers()
    {
        var metaList = await _client.Rest.Fundamentals.GetMeta(["AAPL", "MSFT"]);
        Assert.That(metaList, Is.Not.Null);
        Assert.That(metaList, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task FundamentalMetaAllTickers()
    {
        var metaList = await _client.Rest.Fundamentals.GetMeta();
        Assert.That(metaList, Is.Not.Null);
        Assert.That(metaList, Has.Length.Positive);
    }

    [Test]
    [TestCase("AAPL")]
    [TestCase("MSFT")]
    [TestCase("KO")]
    public async Task FundamentalStatements(string ticker)
    {
        var statements = await _client.Rest.Fundamentals.GetStatements(ticker);
        Assert.That(statements, Is.Not.Null);
        Assert.That(statements, Has.Length.Positive);
        foreach (var statement in statements)
        {
            Assert.That(statement.Year, Is.GreaterThan(2000));
            Assert.That(statement.Date, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(statement.StatementData, Is.Not.Null);
        }
    }

    [Test]
    public async Task FundamentalStatementsWithOptions()
    {
        var interval = new DateTimeInterval(DateTimeOffset.UtcNow - TimeSpan.FromDays(365 * 3), DateTimeOffset.UtcNow);
        var statements = await _client.Rest.Fundamentals.GetStatements("AAPL", interval, asReported: true, sort: "-date");
        Assert.That(statements, Is.Not.Null);
        Assert.That(statements, Has.Length.Positive);
        foreach (var statement in statements)
        {
            Assert.That(statement.Date, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(statement.StatementData, Is.Not.Null);
        }
    }

    [Test]
    [TestCase("AAPL")]
    [TestCase("MSFT")]
    [TestCase("KO")]
    public async Task FundamentalDaily(string ticker)
    {
        var dailyMetrics = await _client.Rest.Fundamentals.GetDaily(ticker);
        Assert.That(dailyMetrics, Is.Not.Null);
        Assert.That(dailyMetrics, Has.Length.Positive);
        foreach (var metric in dailyMetrics)
        {
            Assert.That(metric.Date, Is.Not.EqualTo(default(DateTimeOffset)));
        }
    }

    [Test]
    public async Task FundamentalDailyWithOptions()
    {
        var interval = new DateTimeInterval(DateTimeOffset.UtcNow - TimeSpan.FromDays(90), DateTimeOffset.UtcNow);
        var dailyMetrics = await _client.Rest.Fundamentals.GetDaily("AAPL", interval, sort: "-date");
        Assert.That(dailyMetrics, Is.Not.Null);
        Assert.That(dailyMetrics, Has.Length.Positive);
        foreach (var metric in dailyMetrics)
        {
            Assert.That(metric.Date, Is.Not.EqualTo(default(DateTimeOffset)));
            if (metric.MarketCap.HasValue)
                Assert.That(metric.MarketCap.Value, Is.GreaterThan(0));
        }
    }

    [Test]
    public void FundamentalDefinitionDeserialization()
    {
        var json = """
        [
          {
            "dataCode": "rps",
            "name": "Revenue Per Share",
            "description": "Revenue per share",
            "statementType": "overview",
            "units": "$"
          },
          {
            "dataCode": "roa",
            "name": "Return on Assets ROA",
            "description": "Net Income/Total Assets",
            "statementType": "overview",
            "units": "%"
          },
          {
            "dataCode": "assetTurnover",
            "name": "Asset Turnover",
            "description": "Revenue over assets",
            "statementType": "overview",
            "units": null
          }
        ]
        """;

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = System.Text.Json.JsonSerializer.Deserialize<FundamentalDefinition[]>(json, options);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Length.EqualTo(3));
        Assert.That(result![0].DataCode, Is.EqualTo("rps"));
        Assert.That(result[0].Name, Is.EqualTo("Revenue Per Share"));
        Assert.That(result[0].Units, Is.EqualTo("$"));
        Assert.That(result[2].Units, Is.Null);
    }

    [Test]
    public void FundamentalMetaDeserialization()
    {
        var json = """
        [
          {
            "permaTicker": "US000000000038",
            "ticker": "aapl",
            "name": "Apple Inc",
            "isActive": true,
            "isADR": false,
            "sector": "Technology",
            "industry": "Consumer Electronics",
            "sicCode": 3571,
            "sicSector": "Manufacturing",
            "sicIndustry": "Electronic Computers",
            "reportingCurrency": "usd",
            "location": "California, USA",
            "companyWebsite": "http://www.apple.com",
            "secFilingWebsite": "https://www.sec.gov/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193",
            "statementLastUpdated": "2026-08-01T01:01:20.677Z",
            "dailyLastUpdated": "2026-08-20T01:01:20.677Z"
          }
        ]
        """;

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = System.Text.Json.JsonSerializer.Deserialize<FundamentalMeta[]>(json, options);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Length.EqualTo(1));
        var meta = result![0];
        Assert.That(meta.PermaTicker, Is.EqualTo("US000000000038"));
        Assert.That(meta.Ticker, Is.EqualTo("aapl"));
        Assert.That(meta.Name, Is.EqualTo("Apple Inc"));
        Assert.That(meta.IsActive, Is.True);
        Assert.That(meta.IsADR, Is.False);
        Assert.That(meta.SicCode, Is.EqualTo(3571));
        Assert.That(meta.StatementLastUpdated, Is.Not.Null);
        Assert.That(meta.DailyLastUpdated, Is.Not.Null);
    }

    [Test]
    public void FundamentalStatementDeserialization()
    {
        var json = """
        [
          {
            "date": "2026-06-27",
            "year": 2026,
            "quarter": 3,
            "statementData": {
              "cashFlow": [
                { "dataCode": "ncff", "value": -32640000000.0 },
                { "dataCode": "ncfi", "value": -7757000000.0 }
              ],
              "balanceSheet": [
                { "dataCode": "totalAssets", "value": 323888000000.0 }
              ],
              "incomeStatement": [
                { "dataCode": "netIncome", "value": 12674000000.0 }
              ],
              "overview": [
                { "dataCode": "peRatio", "value": 29.18 }
              ]
            }
          }
        ]
        """;

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = System.Text.Json.JsonSerializer.Deserialize<FundamentalStatement[]>(json, options);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Length.EqualTo(1));
        var stmt = result![0];
        Assert.That(stmt.Year, Is.EqualTo(2026));
        Assert.That(stmt.Quarter, Is.EqualTo(3));
        Assert.That(stmt.StatementData.CashFlow, Has.Length.EqualTo(2));
        Assert.That(stmt.StatementData.CashFlow![0].DataCode, Is.EqualTo("ncff"));
        Assert.That(stmt.StatementData.CashFlow[0].Value, Is.EqualTo(-32640000000.0));
        Assert.That(stmt.StatementData.BalanceSheet![0].Value, Is.EqualTo(323888000000.0));
    }

    [Test]
    public void FundamentalDailyMetricDeserialization()
    {
        var json = """
        [
          {
            "date": "2023-08-21T00:00:00.000Z",
            "marketCap": 2765734959680.0,
            "enterpriseVal": 2812532959680.0,
            "peRatio": 29.1867344837,
            "pbRatio": 45.8860364283,
            "trailingPEG1Y": 5.0034401972
          }
        ]
        """;

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = System.Text.Json.JsonSerializer.Deserialize<FundamentalDailyMetric[]>(json, options);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Length.EqualTo(1));
        var metric = result![0];
        Assert.That(metric.MarketCap, Is.EqualTo(2765734959680.0));
        Assert.That(metric.EnterpriseVal, Is.EqualTo(2812532959680.0));
        Assert.That(metric.PeRatio, Is.EqualTo(29.1867344837));
        Assert.That(metric.PbRatio, Is.EqualTo(45.8860364283));
        Assert.That(metric.TrailingPEG1Y, Is.EqualTo(5.0034401972));
    }
}

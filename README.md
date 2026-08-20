# DotTiingo

[![NuGet](https://img.shields.io/nuget/v/DotTiingo)](https://www.nuget.org/packages/DotTiingo)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DotTiingo)](https://www.nuget.org/packages/DotTiingo)

A simple .NET library for accessing the [Tiingo](https://www.tiingo.com) financial data API.

- **Version:** 1.3.0
- **Target Framework:** .NET 10
- **License:** MIT

---

## Installation

```bash
dotnet add package DotTiingo
```

Or via the NuGet Package Manager:

```
NuGet\Install-Package DotTiingo
```

---

## Setup

Create a `TiingoClient` with an `HttpClient` and your Tiingo API token.

```csharp
using DotTiingo;

using var httpClient = new HttpClient();
var client = new TiingoClient(httpClient, "YOUR_TIINGO_TOKEN");
```

> Your token can be found in your [Tiingo account settings](https://www.tiingo.com/account/api/token).

---

## REST API

Access REST endpoints via `client.Rest`.

### End-of-Day Prices

```csharp
var prices = await client.Rest.EndOfDay.GetEndOfDayPrices("AAPL", interval: null, resampleFreq: null, sortBy: null);
var meta   = await client.Rest.EndOfDay.GetEndOfDayMeta("AAPL");
```

### IEX (Intraday)

```csharp
// Current top-of-book and last price
var quotes = await client.Rest.Iex.GetIexCurrentTopOfBookAndLastPrice(new[] { "AAPL", "MSFT" });

// Historical intraday prices
var history = await client.Rest.Iex.GetIexHistoricalPrices("AAPL", interval: null, resampleFreq: "1hour", afterHours: false, forceFill: false);
```

### Equity Realtime (Beta)

> **Beta:** Equity Realtime endpoints are currently in beta on Tiingo.

```csharp
// Current reference price and liquidity snapshot
var snapshots = await client.Rest.EquityRealtime.GetCurrentReferencePriceAndLiquidity(new[] { "AAPL", "MSFT" });

// Historical intraday prices
var history = await client.Rest.EquityRealtime.GetHistoricalPrices("AAPL", interval: null, resampleFreq: "1hour", afterHours: false, forceFill: false);
```

### Forex (Beta)

> **Beta:** Forex endpoints are currently in beta on Tiingo.

```csharp
// Current top-of-book
var quotes = await client.Rest.Forex.GetCurrentTopOfBook(new[] { "eurusd", "gbpusd" });

// Open, high, low, close (OHLC) prices
var prices = await client.Rest.Forex.GetOpenHighLowClose("eurusd", resampleFreq: "1hour", interval: null);
```

### Crypto

```csharp
// Prices
var prices = await client.Rest.Crypto.GetCryptoPrices(new[] { "btcusdt" }, exchanges: null, interval: null, resampleFreq: null);

// Metadata
var meta = await client.Rest.Crypto.GetCryptoMeta(new[] { "btcusdt" });
```

### News

```csharp
var articles = await client.Rest.News.GetNews(
    tickers:  new[] { "AAPL" },
    sources:  null,
    interval: null,
    limit:    10,
    offset:   null,
    sortBy:   null
);
```

### Fundamentals

```csharp
// Fundamental definitions
var definitions = await client.Rest.Fundamentals.GetDefinitions(new[] { "AAPL" });

// Metadata
var meta = await client.Rest.Fundamentals.GetMeta(new[] { "AAPL", "MSFT" });

// Historical financial statements (Income Statement, Balance Sheet, Cash Flow, Overview)
var statements = await client.Rest.Fundamentals.GetStatements("AAPL", interval: null, asReported: false, sort: "-date");

// Historical daily fundamental metrics (Market Cap, P/E, P/B, PEG, Enterprise Value)
var daily = await client.Rest.Fundamentals.GetDaily("AAPL", interval: null, sort: "-date");
```

---

## WebSocket API

Access real-time streams via `client.WebSocket`. Each connection returns an `ITiingoWebSocketConnection` with an `OnResponseReceived` event.

### Available Feeds

| Feed                   | Method                                                 | Threshold Levels                           |
|------------------------|--------------------------------------------------------|--------------------------------------------|
| Crypto                 | `client.WebSocket.Crypto.Connect(level, ct)`           | `Trade`, `QuoteAndTrade`                   |
| IEX                    | `client.WebSocket.Iex.Connect(level, ct)`              | `AllUpdates`, `Filtered`, `ReferencePrice` |
| Equity Realtime (Beta) | `client.WebSocket.EquityRealtime.Connect(level, ct)`   | `LiquidityRiskMetric`, `ReferencePrice`    |
| Forex (Beta)           | `client.WebSocket.Forex.Connect(level, ct)`            | `TopOfBook`                                |

### Example — Live Crypto Trades

```csharp
using DotTiingo.Api.WebSocket;
using DotTiingo.Model.WebSocket.Response;

using var conn = await client.WebSocket.Crypto.Connect(CryptoThresholdLevel.Trade, CancellationToken.None);

conn.OnResponseReceived += (_, response) =>
{
    if (response is DataResponse { Data: CryptoTradeUpdate trade })
    {
        Console.WriteLine($"{trade.Ticker} — ${trade.LastPrice:N2} ({trade.Exchange})");
    }
};

await Task.Delay(Timeout.Infinite);
```

---

## API Reference

### REST

| Property              | Description                                                  |
|-----------------------|--------------------------------------------------------------|
| `Rest.EndOfDay`       | End-of-day prices and metadata                               |
| `Rest.Iex`            | Intraday top-of-book and historical IEX                      |
| `Rest.EquityRealtime` | Consolidated equity reference price, liquidity, and intraday (Beta) |
| `Rest.Forex`          | Top-of-book and historical/current OHLC (Beta)               |
| `Rest.Crypto`         | Crypto prices and metadata                                   |
| `Rest.News`           | News articles filtered by ticker/source                      |
| `Rest.Fundamentals`   | Fundamental definitions, metadata, statements, and daily metrics |

### WebSocket

| Property                   | Description                                                         | Threshold Enum                |
|----------------------------|---------------------------------------------------------------------|-------------------------------|
| `WebSocket.Crypto`         | Real-time crypto feed                                               | `CryptoThresholdLevel`        |
| `WebSocket.Iex`            | Real-time IEX feed                                                  | `IexThresholdLevel`           |
| `WebSocket.EquityRealtime` | Real-time consolidated equity reference price and liquidity (Beta) | `EquityRealtimeThresholdLevel`|
| `WebSocket.Forex`          | Real-time forex feed (Beta)                                         | `ForexThresholdLevel`         |

---

## License

MIT — see [LICENSE.txt](LICENSE.txt).

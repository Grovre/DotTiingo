# DotTiingo

[![NuGet](https://img.shields.io/nuget/v/DotTiingo)](https://www.nuget.org/packages/DotTiingo)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DotTiingo)](https://www.nuget.org/packages/DotTiingo)

A simple .NET library for accessing the [Tiingo](https://www.tiingo.com) financial data API.

- **Version:** 1.2.0
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
var client = new TiingoClient(httpClient, &quot;YOUR_TIINGO_TOKEN&quot;);
```

> Your token can be found in your [Tiingo account settings](https://www.tiingo.com/account/api/token).

---

## REST API

Access REST endpoints via `client.Rest`.

### End-of-Day Prices

```csharp
var prices = await client.Rest.EndOfDay.GetEndOfDayPrices(&quot;AAPL&quot;, interval: null, resampleFreq: null, sortBy: null);
var meta   = await client.Rest.EndOfDay.GetEndOfDayMeta(&quot;AAPL&quot;);
```

### IEX (Intraday)

```csharp
// Current top-of-book and last price
var quotes = await client.Rest.Iex.GetIexCurrentTopOfBookAndLastPrice(new[] { &quot;AAPL&quot;, &quot;MSFT&quot; });

// Historical intraday prices
var history = await client.Rest.Iex.GetIexHistoricalPrices(&quot;AAPL&quot;, interval: null, resampleFreq: &quot;1hour&quot;, afterHours: false, forceFill: false);
```

### Forex

```csharp
// Current top-of-book
var quotes = await client.Rest.Forex.GetCurrentTopOfBook(new[] { &quot;eurusd&quot;, &quot;gbpusd&quot; });

// Open, high, low, close (OHLC) prices
var prices = await client.Rest.Forex.GetOpenHighLowClose(&quot;eurusd&quot;, resampleFreq: &quot;1hour&quot;, interval: null);
```

### Crypto

```csharp
// Prices
var prices = await client.Rest.Crypto.GetCryptoPrices(new[] { &quot;btcusdt&quot; }, exchanges: null, interval: null, resampleFreq: null);

// Metadata
var meta = await client.Rest.Crypto.GetCryptoMeta(new[] { &quot;btcusdt&quot; });
```

### News

```csharp
var articles = await client.Rest.News.GetNews(
    tickers:  new[] { &quot;AAPL&quot; },
    sources:  null,
    interval: null,
    limit:    10,
    offset:   null,
    sortBy:   null
);
```

---

## WebSocket API

Access real-time streams via `client.WebSocket`. Each connection returns an `ITiingoWebSocketConnection` with an `OnResponseReceived` event.

### Available Feeds

| Feed   | Method                                        | Threshold Levels                           |
|--------|-----------------------------------------------|--------------------------------------------|
| Crypto | `client.WebSocket.Crypto.Connect(level, ct)`  | `Trade`, `QuoteAndTrade`                   |
| IEX    | `client.WebSocket.Iex.Connect(level, ct)`     | `AllUpdates`, `Filtered`, `ReferencePrice` |
| Forex  | `client.WebSocket.Forex.Connect(level, ct)`   | `TopOfBook`                                |

### Example — Live Crypto Trades

```csharp
using DotTiingo.Api.WebSocket;
using DotTiingo.Model.WebSocket.Response;

using var conn = await client.WebSocket.Crypto.Connect(CryptoThresholdLevel.Trade, CancellationToken.None);

conn.OnResponseReceived += (_, response) =&gt;
{
    if (response is DataResponse { Data: CryptoTradeUpdate trade })
    {
        Console.WriteLine($&quot;{trade.Ticker} — ${trade.LastPrice:N2} ({trade.Exchange})&quot;);
    }
};

await Task.Delay(Timeout.Infinite);
```

---

## API Reference

### REST

| Property        | Description                             |
|-----------------|-----------------------------------------|
| `Rest.EndOfDay` | End-of-day prices and metadata          |
| `Rest.Iex`      | Intraday top-of-book and historical IEX |
| `Rest.Forex`    | Top-of-book and historical/current OHLC |
| `Rest.Crypto`   | Crypto prices and metadata              |
| `Rest.News`     | News articles filtered by ticker/source |

### WebSocket

| Property            | Description              | Threshold Enum         |
|---------------------|--------------------------|------------------------|
| `WebSocket.Crypto`  | Real-time crypto feed    | `CryptoThresholdLevel` |
| `WebSocket.Iex`     | Real-time IEX feed       | `IexThresholdLevel`    |
| `WebSocket.Forex`   | Real-time forex feed     | `ForexThresholdLevel`  |

---

## License

MIT — see [LICENSE.txt](LICENSE.txt).

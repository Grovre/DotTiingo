// This example demonstrates using the DotTiingo library to connect to the Tiingo Crypto WebSocket stream
// (TiingoClient.WebSocket.Crypto) and plot the last 100 BTC/USD trade prices in real time in the console.

using ConsolePlot;
using ConsolePlot.Plotting;
using DotTiingo;
using DotTiingo.Api.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using Microsoft.Extensions.Configuration;
using System.Text;

var cfg = new ConfigurationBuilder()
    .AddUserSecrets("5766a622-71e1-4b18-9c78-4cccf5ce4977")
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var tiingoToken = cfg["tiingo_token"]
    ?? throw new Exception("Tiingo token not found in configuration.");

using var httpClient = new HttpClient();
var client = new TiingoClient(httpClient, tiingoToken);

// Connect to the Tiingo crypto trade WebSocket stream
using var conn = await client.WebSocket.Crypto.Connect(CryptoThresholdLevel.Trade, CancellationToken.None);

const int maxTrades = 100;
var prices = new Queue<double>(maxTrades);
var @lock = new Lock();
CryptoTradeUpdate? latestTrade = null;

// Listen for incoming trade updates and keep the last 100 BTC/USD prices
conn.OnResponseReceived += (_, r) =>
{
    if (r is not DataResponse { Data: CryptoTradeUpdate ctu })
        return;

    if (!ctu.Ticker.Equals("btcusdt", StringComparison.OrdinalIgnoreCase))
        return;

    lock (@lock)
    {
        if (prices.Count >= maxTrades)
            prices.Dequeue();

        prices.Enqueue(ctu.LastPrice);
        latestTrade = ctu;
    }
};

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("Connected to Tiingo Crypto WebSocket. Waiting for BTC/USD trades...");

// Periodically redraw the price chart of the last 100 trades
await Task.Run(() =>
{
    while (true)
    {
        Thread.Sleep(1000);

        double[] currentPrices;
        CryptoTradeUpdate? currentTrade;

        lock (@lock)
        {
            if (prices.Count == 0)
                continue;

            currentPrices = prices.ToArray();
            currentTrade = latestTrade;
        }

        double[] xs = Enumerable.Range(1, currentPrices.Length).Select(i => (double)i).ToArray();

        var plot = new Plot(Console.WindowWidth, Console.WindowHeight - 1);
        plot.Ticks.Labels.Format = "N2";
        plot.AddSeries(xs, currentPrices);
        plot.Draw();

        Console.Clear();
        if (currentTrade != null)
        {
            Console.Clear();
            Console.WriteLine($"[BTC/USD] Last Price: ${currentTrade.LastPrice:N2} | Size: {currentTrade.LastSize:N4} | Exchange: {currentTrade.Exchange} | Points: {currentPrices.Length}/{maxTrades}");
        }
        plot.Render();
    }
});
// This example demonstrates using the DotTiingo library to connect to the Tiingo Crypto WebSocket stream
// (TiingoClient.WebSocket.Crypto) and plot the last 100 BTC trade prices from USD/USDT/USDC in real time in the console.

using ConsolePlot;
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

Console.OutputEncoding = Encoding.UTF8;

using var httpClient = new HttpClient();
var client = new TiingoClient(httpClient, tiingoToken);
const int maxTrades = 100;
var priceQueue = new Queue<double>(capacity: maxTrades);

// Connect to the Tiingo crypto trade WebSocket stream
using var conn = await client.WebSocket.Crypto.Connect(CryptoThresholdLevel.Trade, CancellationToken.None);
// Can be recreated using the EventHandler<> also in conn
await foreach (var response in conn.ReceiveEnumerableAsync(CancellationToken.None))
{
    if (response is not DataResponse { Data: CryptoTradeUpdate ctu })
        continue;

    if (ctu.Ticker is not "btcusdt" and not "btcusdc" and not "btcusd")
        continue;
    
    priceQueue.Enqueue(ctu.LastPrice);
    while (priceQueue.Count > maxTrades)
        priceQueue.Dequeue();
    

    var plot = new Plot(Console.WindowWidth, Console.WindowHeight - 3);
    plot.Ticks.Labels.Format = "N2";
    double[] pricePoints = [.. priceQueue.Concat(Enumerable.Repeat((double)ctu.LastPrice, maxTrades - priceQueue.Count))];
    double[] columns = [.. Enumerable.Range(1, pricePoints.Length).Select(i => (double)i)];
    plot.AddSeries(columns, pricePoints);
    plot.Draw();

    Console.Clear();
    Console.WriteLine($"[{ctu.Ticker}] Last Price: ${ctu.LastPrice:N2} | Size: {ctu.LastSize:N4} | Exchange: {ctu.Exchange} | Points: {priceQueue.Count}/{maxTrades}");
    plot.Render();
}
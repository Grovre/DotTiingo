// See https://aka.ms/new-console-template for more information

using ConsolePlot;
using ConsolePlot.Plotting;
using DotTiingo;
using DotTiingo.Api.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;

var cfg = new ConfigurationBuilder()
    .AddUserSecrets("5766a622-71e1-4b18-9c78-4cccf5ce4977")
    .AddEnvironmentVariables()
    .Build();
var tiingoToken = cfg["tiingo_token"]
    ?? throw new Exception("Tiingo token not found in configuration.");

using var httpClient = new HttpClient();
var client = new TiingoClient(httpClient, tiingoToken);

using var conn = await client.WebSocket.Crypto.Connect(CryptoThresholdLevel.Trade, CancellationToken.None);

double[] xs = [1, 2, 3, 4, 5, 6, 7, 8, 9];
double[] ys = Enumerable.Repeat(0.0, xs.Length).ToArray();
var @lock = new Lock();

conn.OnResponseReceived += (_, r) =>
{
    if (r is not DataResponse dr)
        return;

    if (dr.Data is not CryptoTradeUpdate ctu)
        return;

    var lastSize = ctu.LastSize;
    while (lastSize < 1.0)
        lastSize *= 10;

    var d = lastSize.ToString("N0")[0] - '0';

    if (d == 0)
        return;

    lock (@lock)
    {
        ys[d - 1]++;
    }
};

await Task.Run(() =>
{
    while (true)
    {
        Thread.Sleep(1000);
        Console.OutputEncoding = Encoding.UTF8;
        var plot = new Plot(Console.WindowWidth, Console.WindowHeight);
        plot.Ticks.Labels.Format = "N0";
        lock (@lock)
        {
            plot.AddSeries(xs, ys);
            plot.Draw();
            Console.Clear();
            plot.Render();
        }
    }
});
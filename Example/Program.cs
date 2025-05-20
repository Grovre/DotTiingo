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

using var conn = await client.WebSocket.Forex.Connect(ForexThresholdLevel.TopOfBook, CancellationToken.None);

Console.OutputEncoding = Encoding.UTF8;
var plot = new Plot(Console.WindowWidth, Console.WindowHeight);
plot.Ticks.Labels.Format = "N5";
var xs = new LinkedList<double>();
var ys = new LinkedList<double>();
var start = DateTime.Now;
var @lock = new Lock();

conn.OnResponseReceived += (_, r) =>
{
    if (r is not DataResponse dr)
        return;

    if (dr.Data is not ForexQuoteUpdate fqu)
        return;

    if (fqu.Ticker != "eurusd")
        return;

    var elapsedSecs = (DateTime.Now - start).TotalSeconds;
    lock (@lock)
    {
        xs.AddLast(elapsedSecs);
        ys.AddLast(fqu.AskPrice);
    }
};

await Task.Run(() =>
{
    var lastXs = 0;
    var lastYs = 0;
    while (true)
    {
        Thread.Sleep(1000);
        plot.Series.Clear();
        lock (@lock)
        {
            Debug.Assert(xs.Count == ys.Count);

            if (xs.Count == 0 || ys.Count == 0)
                continue;

            if (lastXs == xs.Count && lastYs == ys.Count)
                continue;

            lastXs = xs.Count;
            lastYs = ys.Count;

            plot.AddSeries(xs, ys);
            plot.Draw();
            Console.Clear();
            plot.Render();
        }
    }
});

await Task.Delay(-1);
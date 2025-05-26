using DotTiingo;
using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
using DotTiingo.Api.WebSocket;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing;

[TestFixture]
public class WebSocketApiTests
{
    private TiingoClient _client = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        var httpClient = new HttpClient();
        _client = new TiingoClient(httpClient, TestConfiguration.TiingoToken);
    }

    [Test]
    public async Task Crypto()
    {
        using var conn = await _client.WebSocket.Crypto.Connect(CryptoThresholdLevel.QuoteAndTrade, CancellationToken.None);

        var datum = 0;
        var utilities = 0;
        var messages = new List<AbstractResponse>();
        conn.OnResponseReceived += (sender, response) =>
        {
            messages.Add(response);
            if (response is UtilityResponse utility)
                utilities++;
            else if (response is DataResponse data)
                datum++;
            else
            {
                conn.Dispose();
                Assert.Fail($"Unknown response type: {response.GetType()}. Message type: {response.MessageType}");
            }
        };

        var timedOut = !SpinWait.SpinUntil(
            () => datum + utilities > 100, TimeSpan.FromSeconds(10));

        conn.Dispose();
        if (timedOut)
            Assert.Fail("Not enough responses received.");

        Assert.That(datum + utilities, Is.Positive);
    }

    [Test]
    public async Task Forex()
    {
        var now = DateTime.UtcNow;
        if ((now.DayOfWeek == DayOfWeek.Friday && now.TimeOfDay >= TimeSpan.FromHours(22)) ||
            (now.DayOfWeek == DayOfWeek.Saturday) ||
            (now.DayOfWeek == DayOfWeek.Sunday && now.TimeOfDay < TimeSpan.FromHours(22)))
            Assert.Fail("Markets are closed");

        using var conn = await _client.WebSocket.Forex.Connect(ForexThresholdLevel.TopOfBook, CancellationToken.None);

        var datum = 0;
        var utilities = 0;
        var messages = new List<AbstractResponse>();
        conn.OnResponseReceived += (sender, response) =>
        {
            messages.Add(response);
            if (response is UtilityResponse utility)
                utilities++;
            else if (response is DataResponse data)
                datum++;
            else
            {
                conn.Dispose();
                Assert.Fail($"Unknown response type: {response.GetType()}. Message type: {response.MessageType}");
            }
        };

        var timedOut = !SpinWait.SpinUntil(
            () => datum + utilities > 100, TimeSpan.FromSeconds(10));

        conn.Dispose();
        if (timedOut)
            Assert.Fail("Not enough responses received.");

        Assert.That(datum + utilities, Is.Positive);
    }

    [Test]
    public async Task Iex()
    {
        var open = new TimeOnly(13, 30);  // 9:30 AM ET in UTC
        var close = new TimeOnly(20, 0);  // 4:00 PM ET in UTC
        var now = DateTime.UtcNow;

        if (now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ||
            now.TimeOfDay < open.ToTimeSpan() || now.TimeOfDay >= close.ToTimeSpan())
        {
            Assert.Fail("Markets are closed");
        }

        using var conn = await _client.WebSocket.Iex.Connect(IexThresholdLevel.ReferencePrice, CancellationToken.None);

        var datum = 0;
        var utilities = 0;
        var messages = new List<AbstractResponse>();
        conn.OnResponseReceived += (sender, response) =>
        {
            messages.Add(response);
            if (response is UtilityResponse utility)
                utilities++;
            else if (response is DataResponse data)
                datum++;
            else
            {
                conn.Dispose();
                Assert.Fail($"Unknown response type: {response.GetType()}. Message type: {response.MessageType}");
            }
        };

        var timedOut = !SpinWait.SpinUntil(
            () => datum + utilities > 100, TimeSpan.FromSeconds(10));

        conn.Dispose();
        if (timedOut)
            Assert.Fail("Not enough responses received.");

        Assert.That(datum + utilities, Is.Positive);
    }
}

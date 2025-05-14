using DotTiingo;
using DotTiingo.Model.WebSocket;
using DotTiingo.Model.WebSocket.Response;
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
        using var conn = await _client.WebSocket.Crypto.Connect(2, CancellationToken.None);

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
                Assert.Fail($"Unknown response type: {response.GetType()}. Message type: {response.MessageType}");
        };

        var timedOut = !SpinWait.SpinUntil(
            () => datum + utilities > 100, TimeSpan.FromSeconds(10));

        conn.Dispose();
        if (timedOut)
            Assert.Fail("Not enough responses received.");

        Assert.That(datum, Is.Positive);
        Assert.That(utilities, Is.Positive);
    }

    [Test]
    public async Task Forex()
    {
        var conn = await _client.WebSocket.Forex.Connect(2, CancellationToken.None);

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
                Assert.Fail($"Unknown response type: {response.GetType()}. Message type: {response.MessageType}");
        };

        var timedOut = !SpinWait.SpinUntil(
            () => datum + utilities > 100, TimeSpan.FromSeconds(10));

        conn.Dispose();
        if (timedOut)
            Assert.Fail("Not enough responses received.");

        Assert.That(datum, Is.Positive);
        Assert.That(utilities, Is.Positive);
    }
}

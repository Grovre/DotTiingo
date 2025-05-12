using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing;

[SetUpFixture]
public class TestConfiguration
{
    public static string TiingoToken { get; private set; } = null!;
    public static IConfiguration Configuration { get; private set; } = null!;

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        var cfg = new ConfigurationBuilder()
            .AddUserSecrets("941f8045-18f7-4d7e-8437-506cec497b1b")
            .Build();

        Configuration = cfg;
        TiingoToken = cfg["tiingo_token"]
            ?? throw new Exception("Tiingo token not found in user secrets.");
    }
}

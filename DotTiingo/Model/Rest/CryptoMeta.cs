using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.Rest;

/// <summary>
/// Represents metadata for a cryptocurrency as returned by the Tiingo API.
/// </summary>
/// <param name="Ticker">The ticker symbol for the cryptocurrency (e.g., "btcusd").</param>
/// <param name="BaseCurrency">The base currency (e.g., "btc").</param>
/// <param name="QuoteCurrency">The quote currency (e.g., "usd").</param>
/// <param name="Name">The full name of the cryptocurrency.</param>
/// <param name="Description">A description of the cryptocurrency, if available.</param>
public record CryptoMeta(
    string Ticker,
    string BaseCurrency,
    string QuoteCurrency,
    string Name,
    string? Description);
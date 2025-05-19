using DotTiingo.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace DotTiingo.Api;

/// <summary>
/// Aggregates all Tiingo REST API endpoints.
/// </summary>
public interface ITiingoRestApi : IDisposable
{
    /// <summary>
    /// Provides access to end-of-day price and meta data endpoints.
    /// </summary>
    ITiingoRestEndOfDayApi EndOfDay { get; }
    /// <summary>
    /// Provides access to IEX price endpoints.
    /// </summary>
    ITiingoRestIexApi Iex { get; }
    /// <summary>
    /// Provides access to cryptocurrency price and meta data endpoints.
    /// </summary>
    ITiingoRestCryptoApi Crypto { get; }
    /// <summary>
    /// Provides access to news endpoints.
    /// </summary>
    ITiingoRestNewsApi News { get; }
}

/// <summary>
/// Implementation of <see cref="ITiingoRestApi"/> for accessing Tiingo REST endpoints.
/// </summary>
public sealed class RestApi : ITiingoRestApi
{
    private readonly HttpClient _httpClient;
    /// <inheritdoc/>
    public ITiingoRestEndOfDayApi EndOfDay { get; }
    /// <inheritdoc/>
    public ITiingoRestIexApi Iex { get; }
    /// <inheritdoc/>
    public ITiingoRestCryptoApi Crypto { get; }
    /// <inheritdoc/>
    public ITiingoRestNewsApi News { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RestApi"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="token">The Tiingo API token.</param>
    public RestApi(HttpClient httpClient, string token)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Authorization = new("Token", token);

        EndOfDay = new RestEndOfDayApi(_httpClient);
        Iex = new RestIexApi(_httpClient);
        Crypto = new RestCryptoApi(_httpClient);
        News = new RestNewsApi(_httpClient);
    }

    /// <summary>
    /// Disposes the underlying HTTP client and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
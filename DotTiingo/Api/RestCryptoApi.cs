using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotTiingo.Model.Rest;
using DotTiingo.Extensions;

namespace DotTiingo.Api;

/// <summary>
/// Provides access to cryptocurrency price and meta data endpoints.
/// </summary>
public interface ITiingoRestCryptoApi
{
    /// <summary>
    /// Gets cryptocurrency prices for the specified tickers and options.
    /// </summary>
    /// <param name="tickers">The cryptocurrency tickers.</param>
    /// <param name="exchanges">The exchanges to filter by (optional).</param>
    /// <param name="interval">The date interval (optional).</param>
    /// <param name="resampleFreq">The resample frequency (optional).</param>
    /// <returns>Array of <see cref="CryptoPrice"/>.</returns>
    Task<CryptoPrice[]> GetCryptoPrices(IEnumerable<string> tickers, IEnumerable<string>? exchanges, DateTimeInterval? interval, string? resampleFreq);

    /// <summary>
    /// Gets cryptocurrency meta data for the specified tickers.
    /// </summary>
    /// <param name="tickers">The cryptocurrency tickers (optional).</param>
    /// <returns>Array of <see cref="CryptoMeta"/>.</returns>
    Task<CryptoMeta[]> GetCryptoMeta(IEnumerable<string>? tickers);
}

/// <summary>
/// Implementation of <see cref="ITiingoRestCryptoApi"/>.
/// </summary>
public class RestCryptoApi : ITiingoRestCryptoApi
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestCryptoApi"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    public RestCryptoApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public Task<CryptoPrice[]> GetCryptoPrices(IEnumerable<string> tickers, IEnumerable<string>? exchanges, DateTimeInterval? interval, string? resampleFreq)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/crypto/prices?tickers={string.Join(',', tickers)}";
        dynamic content = new ExpandoObject();
        if (exchanges != null)
            content.exchanges = exchanges;
        if (interval != null)
        {
            (content.startDate, content.endDate) = interval.ToTiingoString();
        }
        if (resampleFreq != null)
            content.resampleFreq = resampleFreq;

        var apiResultFactory = new ApiResultFactory<CryptoPrice[]>(_httpClient);
        return apiResultFactory.CreateGet(JsonContent.Create(content), fullUrl);
    }

    /// <inheritdoc/>
    public Task<CryptoMeta[]> GetCryptoMeta(IEnumerable<string>? tickers)
    {
        var queryTickers = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/crypto?{queryTickers}";

        var apiResultFactory = new ApiResultFactory<CryptoMeta[]>(_httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }
}

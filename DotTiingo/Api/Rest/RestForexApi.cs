using System.Dynamic;
using System.Diagnostics.CodeAnalysis;
using DotTiingo.Extensions;
using DotTiingo.Model.Rest;

namespace DotTiingo.Api.Rest;

/// <summary>
/// Provides access to Forex price endpoints.
/// </summary>
[Experimental("TNGOBETA")]
public interface ITiingoRestForexApi
{
    /// <summary>
    /// Gets top-of-book and last price data for the specified forex tickers.
    /// </summary>
    /// <param name="tickers">The forex tickers (optional).</param>
    /// <returns>Array of <see cref="ForexCurrentTopOfBook"/>.</returns>
    public Task<ForexCurrentTopOfBook[]> GetCurrentTopOfBook(IEnumerable<string>? tickers);

    /// <summary>
    /// Gets open, high, low, close (OHLC) prices for a forex pair.
    /// </summary>
    /// <param name="ticker">The forex ticker symbol.</param>
    /// <param name="resampleFreq">The frequency in which data is resampled (optional).</param>
    /// <param name="interval">The date interval to query historical data for (optional).</param>
    /// <returns>Array of <see cref="ForexCurrentOpenHighLowClose"/>.</returns>
    public Task<ForexCurrentOpenHighLowClose[]> GetOpenHighLowClose(string ticker, string? resampleFreq = null, DateTimeInterval? interval = null);
}

/// <summary>
/// Implementation of <see cref="ITiingoRestForexApi"/>.
/// </summary>
[Experimental("TNGOBETA")]
public class RestForexApi : ITiingoRestForexApi
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestForexApi"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    public RestForexApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public Task<ForexCurrentTopOfBook[]> GetCurrentTopOfBook(IEnumerable<string>? tickers)
    {
        var queryTickers = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/fx/top{queryTickers}";

        var responseFactory = new ApiResultFactory<ForexCurrentTopOfBook[]>(_httpClient);
        return responseFactory.CreateGet(null, fullUrl);
    }
    
    /// <inheritdoc/>
    public Task<ForexCurrentOpenHighLowClose[]> GetOpenHighLowClose(string ticker, string? resampleFreq = null, DateTimeInterval? interval = null)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/fx/{ticker}/prices";
        if (resampleFreq != null || interval != null)
        {
            fullUrl += '?';
            if (resampleFreq != null)
                fullUrl += $"resampleFreq={resampleFreq}&";
            if (interval != null)
                fullUrl += $"startDate={interval.Start.ToTiingoString()}&endDate={interval.End.ToTiingoString()}";
        }
        
        var responseFactory = new ApiResultFactory<ForexCurrentOpenHighLowClose[]>(_httpClient);
        return responseFactory.CreateGet(null, fullUrl);
    }
}
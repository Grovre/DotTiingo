using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using DotTiingo.Extensions;
using DotTiingo.Model.Rest;

namespace DotTiingo.Api.Rest;

/// <summary>
/// Provides access to Equity Realtime price and liquidity endpoints.
/// </summary>
[Experimental("TNGOBETA")]
public interface ITiingoRestEquityRealtimeApi
{
    /// <summary>
    /// Gets current reference price and liquidity metrics for the specified equity tickers or all tickers if null.
    /// </summary>
    /// <param name="tickers">The ticker symbols (optional).</param>
    /// <returns>Array of <see cref="EquityRealtimeSnapshot"/>.</returns>
    Task<EquityRealtimeSnapshot[]> GetCurrentReferencePriceAndLiquidity(IEnumerable<string>? tickers = null);

    /// <summary>
    /// Gets current reference price and liquidity metrics for a specific equity ticker.
    /// </summary>
    /// <param name="ticker">The ticker symbol.</param>
    /// <returns>Array of <see cref="EquityRealtimeSnapshot"/>.</returns>
    Task<EquityRealtimeSnapshot[]> GetCurrentReferencePriceAndLiquidity(string ticker);

    /// <summary>
    /// Gets historical intraday prices for a given equity ticker.
    /// </summary>
    /// <param name="ticker">The ticker symbol.</param>
    /// <param name="interval">The date interval (optional).</param>
    /// <param name="resampleFreq">The resample frequency (optional, e.g. "5min", "1hour"). Defaults to "5min" on API if not specified.</param>
    /// <param name="afterHours">Whether to include after-hours data (optional).</param>
    /// <param name="forceFill">Whether to force fill missing data (optional).</param>
    /// <returns>Array of <see cref="EquityHistoricalPrice"/>.</returns>
    Task<EquityHistoricalPrice[]> GetHistoricalPrices(
        string ticker,
        DateTimeInterval? interval = null,
        string? resampleFreq = null,
        bool? afterHours = null,
        bool? forceFill = null);
}

/// <summary>
/// Implementation of <see cref="ITiingoRestEquityRealtimeApi"/>.
/// </summary>
[Experimental("TNGOBETA")]
public class RestEquityRealtimeApi : ITiingoRestEquityRealtimeApi
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestEquityRealtimeApi"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    public RestEquityRealtimeApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public Task<EquityRealtimeSnapshot[]> GetCurrentReferencePriceAndLiquidity(IEnumerable<string>? tickers = null)
    {
        var queryTickers = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/equity/intraday/{queryTickers}";

        var apiResultFactory = new ApiResultFactory<EquityRealtimeSnapshot[]>(_httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }

    /// <inheritdoc/>
    public Task<EquityRealtimeSnapshot[]> GetCurrentReferencePriceAndLiquidity(string ticker)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/equity/intraday/{ticker}";

        var apiResultFactory = new ApiResultFactory<EquityRealtimeSnapshot[]>(_httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }

    /// <inheritdoc/>
    public Task<EquityHistoricalPrice[]> GetHistoricalPrices(
        string ticker,
        DateTimeInterval? interval = null,
        string? resampleFreq = null,
        bool? afterHours = null,
        bool? forceFill = null)
    {
        var queryParams = new List<string> { "columns=open,high,low,close,volume" };
        if (interval != null)
        {
            queryParams.Add($"startDate={interval.Start.ToTiingoString()}");
            queryParams.Add($"endDate={interval.End.ToTiingoString()}");
        }
        if (resampleFreq != null)
            queryParams.Add($"resampleFreq={resampleFreq}");
        if (afterHours != null)
            queryParams.Add($"afterHours={afterHours.Value.ToString().ToLowerInvariant()}");
        if (forceFill != null)
            queryParams.Add($"forceFill={forceFill.Value.ToString().ToLowerInvariant()}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join('&', queryParams) : string.Empty;
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/equity/intraday/{ticker}/prices{queryString}";

        var apiResultFactory = new ApiResultFactory<EquityHistoricalPrice[]>(_httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }
}

using DotTiingo.Extensions;
using DotTiingo.Model.Rest;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Api;

/// <summary>
/// Provides access to IEX price endpoints.
/// </summary>
public interface ITiingoRestIexApi
{
    /// <summary>
    /// Gets the current top-of-book and last price for the specified tickers.
    /// </summary>
    /// <param name="tickers">The ticker symbols (optional).</param>
    /// <returns>Array of <see cref="IexCurrentTopOfBookAndLastPrice"/>.</returns>
    Task<IexCurrentTopOfBookAndLastPrice[]> GetIexCurrentTopOfBookAndLastPrice(IEnumerable<string>? tickers);

    /// <summary>
    /// Gets historical prices for a given ticker and options.
    /// </summary>
    /// <param name="ticker">The ticker symbol.</param>
    /// <param name="interval">The date interval (optional).</param>
    /// <param name="resampleFreq">The resample frequency (optional).</param>
    /// <param name="afterHours">Whether to include after-hours data (optional).</param>
    /// <param name="forceFill">Whether to force fill missing data (optional).</param>
    /// <returns>Array of <see cref="IexHistoricalPrice"/>.</returns>
    Task<IexHistoricalPrice[]> GetIexHistoricalPrices(string ticker, DateTimeInterval? interval, string? resampleFreq, bool? afterHours, bool? forceFill);
}

/// <summary>
/// Implementation of <see cref="ITiingoRestIexApi"/>.
/// </summary>
public class RestIexApi(HttpClient httpClient) : ITiingoRestIexApi
{
    /// <inheritdoc/>
    public Task<IexCurrentTopOfBookAndLastPrice[]> GetIexCurrentTopOfBookAndLastPrice(IEnumerable<string>? tickers)
    {
        var queryTickers = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/iex/{queryTickers}";

        var apiResultFactory = new ApiResultFactory<IexCurrentTopOfBookAndLastPrice[]>(httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }

    /// <inheritdoc/>
    public Task<IexHistoricalPrice[]> GetIexHistoricalPrices(string ticker, DateTimeInterval? interval, string? resampleFreq, bool? afterHours, bool? forceFill)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/iex/{ticker}/prices";
        dynamic content = new ExpandoObject();
        if (interval != null)
        {
            (content.startDate, content.endDate) = (interval.Start.ToTiingoString(), interval.End.ToTiingoString());
        }
        if (resampleFreq != null)
            content.resampleFreq = resampleFreq;
        if (afterHours != null)
            content.afterHours = afterHours.Value;
        if (forceFill != null)
            content.forceFill = forceFill.Value;

        var apiResultFactory = new ApiResultFactory<IexHistoricalPrice[]>(httpClient);
        return apiResultFactory.CreateGet(JsonContent.Create(content), fullUrl);
    }
}

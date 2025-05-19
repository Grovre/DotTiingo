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
/// Provides access to end-of-day price and meta data endpoints.
/// </summary>
public interface ITiingoRestEndOfDayApi
{
    /// <summary>
    /// Gets end-of-day prices for a given ticker and interval.
    /// </summary>
    /// <param name="ticker">The ticker symbol.</param>
    /// <param name="interval">The date interval (optional).</param>
    /// <param name="resampleFreq">The resample frequency (optional).</param>
    /// <param name="sortBy">The field to sort by (optional).</param>
    /// <returns>Array of <see cref="EndOfDayPrice"/>.</returns>
    Task<EndOfDayPrice[]> GetEndOfDayPrices(string ticker, DateTimeInterval? interval, string? resampleFreq, string? sortBy);

    /// <summary>
    /// Gets meta data for a given ticker.
    /// </summary>
    /// <param name="ticker">The ticker symbol.</param>
    /// <returns>The <see cref="EndOfDayMeta"/> data.</returns>
    Task<EndOfDayMeta> GetEndOfDayMeta(string ticker);
}

/// <summary>
/// Implementation of <see cref="ITiingoRestEndOfDayApi"/>.
/// </summary>
public class RestEndOfDayApi : ITiingoRestEndOfDayApi
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestEndOfDayApi"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    public RestEndOfDayApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public Task<EndOfDayPrice[]> GetEndOfDayPrices(string ticker, DateTimeInterval? interval, string? resampleFreq, string? sortBy)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/daily/{ticker}/prices";
        dynamic content = new ExpandoObject();
        if (interval != null)
        {
            (content.startDate, content.endDate) = (interval.Start.ToTiingoString(), interval.End.ToTiingoString());
        }
        if (resampleFreq != null)
            content.resampleFreq = resampleFreq;
        if (sortBy != null)
            content.sort = sortBy;

        var apiResultFactory = new ApiResultFactory<EndOfDayPrice[]>(_httpClient);
        return apiResultFactory.CreateGet(JsonContent.Create(content), fullUrl);
    }

    /// <inheritdoc/>
    public Task<EndOfDayMeta> GetEndOfDayMeta(string ticker)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/daily/{ticker}";
        var apiResultFactory = new ApiResultFactory<EndOfDayMeta>(_httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }
}

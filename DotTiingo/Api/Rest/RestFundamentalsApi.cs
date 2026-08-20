using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DotTiingo.Extensions;
using DotTiingo.Model.Rest;

namespace DotTiingo.Api.Rest;

/// <summary>
/// Provides access to Tiingo Fundamental Data endpoints.
/// </summary>
public interface ITiingoRestFundamentalsApi
{
    /// <summary>
    /// Gets definitions for available fundamental metrics for the specified tickers, or all tickers if null.
    /// </summary>
    /// <param name="tickers">The ticker symbols (optional).</param>
    /// <returns>Array of <see cref="FundamentalDefinition"/>.</returns>
    Task<FundamentalDefinition[]> GetDefinitions(IEnumerable<string>? tickers = null);

    /// <summary>
    /// Gets metadata for fundamental data tickers, or all tickers if null.
    /// </summary>
    /// <param name="tickers">The ticker symbols (optional).</param>
    /// <returns>Array of <see cref="FundamentalMeta"/>.</returns>
    Task<FundamentalMeta[]> GetMeta(IEnumerable<string>? tickers = null);

    /// <summary>
    /// Gets historical statement data for a given ticker or permaTicker.
    /// </summary>
    /// <param name="ticker">The ticker symbol or permaTicker.</param>
    /// <param name="interval">The date interval (optional).</param>
    /// <param name="asReported">Whether to return as-reported data (optional, defaults to false).</param>
    /// <param name="sort">The sort column and direction, e.g. "date" or "-date" (optional).</param>
    /// <returns>Array of <see cref="FundamentalStatement"/>.</returns>
    Task<FundamentalStatement[]> GetStatements(
        string ticker,
        DateTimeInterval? interval = null,
        bool? asReported = null,
        string? sort = null);

    /// <summary>
    /// Gets historical daily fundamental metrics for a given ticker or permaTicker.
    /// </summary>
    /// <param name="ticker">The ticker symbol or permaTicker.</param>
    /// <param name="interval">The date interval (optional).</param>
    /// <param name="sort">The sort column and direction, e.g. "date" or "-date" (optional).</param>
    /// <returns>Array of <see cref="FundamentalDailyMetric"/>.</returns>
    Task<FundamentalDailyMetric[]> GetDaily(
        string ticker,
        DateTimeInterval? interval = null,
        string? sort = null);
}

/// <summary>
/// Implementation of <see cref="ITiingoRestFundamentalsApi"/>.
/// </summary>
public class RestFundamentalsApi : ITiingoRestFundamentalsApi
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestFundamentalsApi"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    public RestFundamentalsApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public Task<FundamentalDefinition[]> GetDefinitions(IEnumerable<string>? tickers = null)
    {
        var queryString = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/fundamentals/definitions{queryString}";

        var apiResultFactory = new ApiResultFactory<FundamentalDefinition[]>(_httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }

    /// <inheritdoc/>
    public Task<FundamentalMeta[]> GetMeta(IEnumerable<string>? tickers = null)
    {
        var queryString = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/fundamentals/meta{queryString}";

        var apiResultFactory = new ApiResultFactory<FundamentalMeta[]>(_httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }

    /// <inheritdoc/>
    public Task<FundamentalStatement[]> GetStatements(
        string ticker,
        DateTimeInterval? interval = null,
        bool? asReported = null,
        string? sort = null)
    {
        var queryParams = new List<string>();
        if (interval != null)
        {
            queryParams.Add($"startDate={interval.Start.ToTiingoString()}");
            queryParams.Add($"endDate={interval.End.ToTiingoString()}");
        }
        if (asReported != null)
            queryParams.Add($"asReported={asReported.Value.ToString().ToLowerInvariant()}");
        if (sort != null)
            queryParams.Add($"sort={sort}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join('&', queryParams) : string.Empty;
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/fundamentals/{ticker}/statements{queryString}";

        var apiResultFactory = new ApiResultFactory<FundamentalStatement[]>(_httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }

    /// <inheritdoc/>
    public Task<FundamentalDailyMetric[]> GetDaily(
        string ticker,
        DateTimeInterval? interval = null,
        string? sort = null)
    {
        var queryParams = new List<string>();
        if (interval != null)
        {
            queryParams.Add($"startDate={interval.Start.ToTiingoString()}");
            queryParams.Add($"endDate={interval.End.ToTiingoString()}");
        }
        if (sort != null)
            queryParams.Add($"sort={sort}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join('&', queryParams) : string.Empty;
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/fundamentals/{ticker}/daily{queryString}";

        var apiResultFactory = new ApiResultFactory<FundamentalDailyMetric[]>(_httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }
}

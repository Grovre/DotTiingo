using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotTiingo.Model.Rest;

namespace DotTiingo.Api;

/// <summary>
/// Provides access to news endpoints.
/// </summary>
public interface ITiingoRestNewsApi
{
    /// <summary>
    /// Gets news articles for the specified tickers, sources, and options.
    /// </summary>
    /// <param name="tickers">The ticker symbols (optional).</param>
    /// <param name="sources">The news sources (optional).</param>
    /// <param name="interval">The date interval (optional).</param>
    /// <param name="limit">The maximum number of articles to return (optional).</param>
    /// <param name="offset">The offset for pagination (optional).</param>
    /// <param name="sortBy">The field to sort by (optional).</param>
    /// <returns>Array of <see cref="NewsArticle"/>.</returns>
    Task<NewsArticle[]> GetNews(IEnumerable<string>? tickers, IEnumerable<string>? sources, DateTimeInterval? interval, int? limit, int? offset, string? sortBy);
}

/// <summary>
/// Implementation of <see cref="ITiingoRestNewsApi"/>.
/// </summary>
public class RestNewsApi : ITiingoRestNewsApi
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestNewsApi"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    public RestNewsApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public Task<NewsArticle[]> GetNews(IEnumerable<string>? tickers, IEnumerable<string>? sources, DateTimeInterval? interval, int? limit, int? offset, string? sortBy)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/news/";
        dynamic content = new ExpandoObject();
        if (tickers != null)
            content.tickers = tickers;
        if (sources != null)
            content.source = sources;
        if (interval != null)
        {
            content.startDate = interval.Start;
            content.endDate = interval.End;
        }
        if (limit != null)
            content.limit = limit;
        if (offset != null)
            content.offset = offset;
        if (sortBy != null)
            content.sortBy = sortBy;

        var apiResultFactory = new ApiResultFactory<NewsArticle[]>(_httpClient);
        return apiResultFactory.CreateGet(JsonContent.Create(content), fullUrl);
    }
}

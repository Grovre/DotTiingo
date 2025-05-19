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

public interface ITiingoRestNewsApi
{
    Task<NewsArticle[]> GetNews(IEnumerable<string>? tickers, IEnumerable<string>? sources, DateTimeInterval? interval, int? limit, int? offset, string? sortBy);
}

public class RestNewsApi(HttpClient httpClient) : ITiingoRestNewsApi
{
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

        var apiResultFactory = new ApiResultFactory<NewsArticle[]>(httpClient);
        return apiResultFactory.CreateGet(JsonContent.Create(content), fullUrl);
    }
}

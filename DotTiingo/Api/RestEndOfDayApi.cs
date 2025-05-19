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

public interface ITiingoRestEndOfDayApi
{
    Task<EndOfDayPrice[]> GetEndOfDayPrices(string ticker, DateTimeInterval? interval, string? resampleFreq, string? sortBy);
    Task<EndOfDayMeta> GetEndOfDayMeta(string ticker);
}

public class RestEndOfDayApi(HttpClient httpClient) : ITiingoRestEndOfDayApi
{
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

        var apiResultFactory = new ApiResultFactory<EndOfDayPrice[]>(httpClient);
        return apiResultFactory.CreateGet(JsonContent.Create(content), fullUrl);
    }

    public Task<EndOfDayMeta> GetEndOfDayMeta(string ticker)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/daily/{ticker}";
        var apiResultFactory = new ApiResultFactory<EndOfDayMeta>(httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }
}

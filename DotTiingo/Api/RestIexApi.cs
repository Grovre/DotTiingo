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

public interface ITiingoRestIexApi
{
    Task<IexCurrentTopOfBookAndLastPrice[]> GetIexCurrentTopOfBookAndLastPrice(IEnumerable<string>? tickers);
    Task<IexHistoricalPrice[]> GetIexHistoricalPrices(string ticker, DateTimeInterval? interval, string? resampleFreq, bool? afterHours, bool? forceFill);
}

public class RestIexApi(HttpClient httpClient) : ITiingoRestIexApi
{
    public Task<IexCurrentTopOfBookAndLastPrice[]> GetIexCurrentTopOfBookAndLastPrice(IEnumerable<string>? tickers)
    {
        var queryTickers = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/iex/{queryTickers}";

        var apiResultFactory = new ApiResultFactory<IexCurrentTopOfBookAndLastPrice[]>(httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }

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

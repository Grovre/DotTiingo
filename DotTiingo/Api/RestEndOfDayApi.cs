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

public interface ITiingoRestEndOfDayApi
{
    Task<EndOfDayPrice[]> GetEndOfDayPrices(string ticker, DateTimeInterval? interval, string? resampleFreq, string? sortBy);
    Task<EndOfDayMeta> GetEndOfDayMeta(string ticker);
}

public class RestEndOfDayApi(HttpClient httpClient) : ITiingoRestEndOfDayApi
{
    public async Task<EndOfDayPrice[]> GetEndOfDayPrices(string ticker, DateTimeInterval? interval, string? resampleFreq, string? sortBy)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/daily/{ticker}/prices";
        dynamic content = new ExpandoObject();
        if (interval != null)
        {
            content.startDate = interval.Start;
            content.endDate = interval.End;
        }
        if (resampleFreq != null)
            content.resampleFreq = resampleFreq;
        if (sortBy != null)
            content.sort = sortBy;

        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        req.Content = JsonContent.Create(content);
        using var response = await httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var eodPrices = await response.Content.ReadFromJsonAsync<EndOfDayPrice[]>();

        return eodPrices
            ?? throw new Exception();
    }

    public async Task<EndOfDayMeta> GetEndOfDayMeta(string ticker)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/daily/{ticker}";

        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        using var response = await httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var meta = await response.Content.ReadFromJsonAsync<EndOfDayMeta>();

        return meta
            ?? throw new Exception();
    }
}

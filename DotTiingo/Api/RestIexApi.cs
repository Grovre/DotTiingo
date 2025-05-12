using DotTiingo.Model;
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
    public async Task<IexCurrentTopOfBookAndLastPrice[]> GetIexCurrentTopOfBookAndLastPrice(IEnumerable<string>? tickers)
    {
        var queryTickers = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/iex/{queryTickers}";

        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        using var response = await httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var price = await response.Content.ReadFromJsonAsync<IexCurrentTopOfBookAndLastPrice[]>();

        return price
            ?? throw new Exception();
    }

    public async Task<IexHistoricalPrice[]> GetIexHistoricalPrices(string ticker, DateTimeInterval? interval, string? resampleFreq, bool? afterHours, bool? forceFill)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/iex/{ticker}/prices";
        dynamic content = new ExpandoObject();
        if (interval != null)
        {
            content.startDate = interval.Start.ToString("yyyy-MM-dd");
            content.endDate = interval.End.ToString("yyyy-MM-dd");
        }
        if (resampleFreq != null)
            content.resampleFreq = resampleFreq;
        if (afterHours != null)
            content.afterHours = afterHours.Value;
        if (forceFill != null)
            content.forceFill = forceFill.Value;

        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        req.Content = JsonContent.Create(content);
        using var response = await httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var prices = await response.Content.ReadFromJsonAsync<IexHistoricalPrice[]>();

        return prices
            ?? throw new Exception();
    }
}

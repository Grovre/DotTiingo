using DotTiingo.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Api;

public interface ITiingoRestCryptoApi
{
    Task<CryptoPrice[]> GetCryptoPrices(IEnumerable<string> tickers, IEnumerable<string>? exchanges, DateTimeInterval? interval, string? resampleFreq);
    Task<CryptoMeta[]> GetCryptoMeta(IEnumerable<string>? tickers);
}

public class RestCryptoApi(HttpClient httpClient) : ITiingoRestCryptoApi
{
    public async Task<CryptoPrice[]> GetCryptoPrices(IEnumerable<string> tickers, IEnumerable<string>? exchanges, DateTimeInterval? interval, string? resampleFreq)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/crypto/prices?tickers={string.Join(',', tickers)}";
        dynamic content = new ExpandoObject();
        if (exchanges != null)
            content.exchanges = exchanges;
        if (interval != null)
        {
            content.startDate = interval.Start.ToString("yyyy-MM-dd");
            content.endDate = interval.End.ToString("yyyy-MM-dd");
        }
        if (resampleFreq != null)
            content.resampleFreq = resampleFreq;

        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        req.Content = JsonContent.Create(content);
        using var response = await httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var cryptoPrices = await response.Content.ReadFromJsonAsync<CryptoPrice[]>();

        return cryptoPrices
            ?? throw new Exception();
    }

    public async Task<CryptoMeta[]> GetCryptoMeta(IEnumerable<string>? tickers)
    {
        var queryTickers = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/crypto?{queryTickers}";

        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        using var response = await httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var meta = await response.Content.ReadFromJsonAsync<CryptoMeta[]>();

        return meta
            ?? throw new Exception();
    }
}

using DotTiingo.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace DotTiingo;

public class RestApi
{
    private const string Url = "https://api.tiingo.com";
    private readonly HttpClient _httpClient;
    private readonly string _token;
    private readonly AuthenticationHeaderValue _authHeader;

    public RestApi(HttpClient httpClient, string token)
    {
        _httpClient = httpClient;
        _token = token;
        _authHeader = new("Token", _token);
    }

    public async Task<EndOfDayPrice[]> GetEndOfDayPrices(string ticker, DateTimeInterval? interval, string? resampleFreq, string? sortBy)
    {
        var fullUrl = $"{Url}/tiingo/daily/{ticker}/prices";
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
        req.Headers.Authorization = _authHeader;
        req.Content = JsonContent.Create(content);

        using var response = await _httpClient.SendAsync(req);
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
        var fullUrl = $"{Url}/tiingo/daily/{ticker}";

        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        req.Headers.Authorization = _authHeader;

        using var response = await _httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var meta = await response.Content.ReadFromJsonAsync<EndOfDayMeta>();

        return meta
            ?? throw new Exception();
    }

    public async Task<NewsArticle[]> GetNews(IEnumerable<string>? tickers, IEnumerable<string>? sources, DateTimeInterval? interval, int? limit, int? offset, string? sortBy)
    {
        var fullUrl = $"{Url}/tiingo/news/";
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

        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        req.Headers.Authorization = _authHeader;
        req.Content = JsonContent.Create(content);

        using var response = await _httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var news = await response.Content.ReadFromJsonAsync<NewsArticle[]>();

        return news
            ?? throw new Exception();
    }

    public async Task<CryptoPrice[]> GetCryptoPrices(IEnumerable<string> tickers, IEnumerable<string>? exchanges, DateTimeInterval? interval, string? resampleFreq)
    {
        var fullUrl = $"{Url}/tiingo/crypto/prices?tickers={string.Join(',', tickers)}";
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
        req.Headers.Authorization = _authHeader;
        req.Content = JsonContent.Create(content);

        using var response = await _httpClient.SendAsync(req);
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
        var fullUrl = $"{Url}/tiingo/crypto?{queryTickers}";

        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        req.Headers.Authorization = _authHeader;

        using var response = await _httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var meta = await response.Content.ReadFromJsonAsync<CryptoMeta[]>();

        return meta
            ?? throw new Exception();
    }

    public async Task<IexCurrentTopOfBookAndLastPrice[]> GetIexCurrentTopOfBookAndLastPrice(IEnumerable<string>? tickers)
    {
        var queryTickers = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{Url}/iex/{queryTickers}";

        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        // Passing it as an auth header causes an error 403, 'please supply a token'
        req.Headers.Authorization = _authHeader;

        using var response = await _httpClient.SendAsync(req);
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
        var fullUrl = $"{Url}/iex/{ticker}/prices";
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
        req.Headers.Authorization = _authHeader;
        req.Content = JsonContent.Create(content);

        using var response = await _httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var prices = await response.Content.ReadFromJsonAsync<IexHistoricalPrice[]>();

        return prices
            ?? throw new Exception();
    }
}
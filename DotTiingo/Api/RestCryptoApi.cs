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

public interface ITiingoRestCryptoApi
{
    Task<CryptoPrice[]> GetCryptoPrices(IEnumerable<string> tickers, IEnumerable<string>? exchanges, DateTimeInterval? interval, string? resampleFreq);
    Task<CryptoMeta[]> GetCryptoMeta(IEnumerable<string>? tickers);
}

public class RestCryptoApi(HttpClient httpClient) : ITiingoRestCryptoApi
{
    public Task<CryptoPrice[]> GetCryptoPrices(IEnumerable<string> tickers, IEnumerable<string>? exchanges, DateTimeInterval? interval, string? resampleFreq)
    {
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/crypto/prices?tickers={string.Join(',', tickers)}";
        dynamic content = new ExpandoObject();
        if (exchanges != null)
            content.exchanges = exchanges;
        if (interval != null)
        {
            (content.startDate, content.endDate) = interval.ToTiingoString();
        }
        if (resampleFreq != null)
            content.resampleFreq = resampleFreq;

        var apiResultFactory = new ApiResultFactory<CryptoPrice[]>(httpClient);
        return apiResultFactory.CreateGet(JsonContent.Create(content), fullUrl);
    }

    public Task<CryptoMeta[]> GetCryptoMeta(IEnumerable<string>? tickers)
    {
        var queryTickers = tickers == null
            ? string.Empty
            : $"?tickers={string.Join(',', tickers)}";
        var fullUrl = $"{TiingoApiHelper.RestBaseUrl}/tiingo/crypto?{queryTickers}";

        var apiResultFactory = new ApiResultFactory<CryptoMeta[]>(httpClient);
        return apiResultFactory.CreateGet(null, fullUrl);
    }
}

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

namespace DotTiingo.Api;

public interface ITiingoRestApi : IDisposable
{
    public ITiingoRestEndOfDayApi EndOfDay { get; }
    public ITiingoRestIexApi Iex { get; }
    public ITiingoRestCryptoApi Crypto { get; }
    public ITiingoRestNewsApi News { get; }
}

public sealed class RestApi : ITiingoRestApi
{
    private readonly HttpClient _httpClient;
    public ITiingoRestEndOfDayApi EndOfDay { get; }
    public ITiingoRestIexApi Iex { get; }
    public ITiingoRestCryptoApi Crypto { get; }
    public ITiingoRestNewsApi News { get; }

    public RestApi(HttpClient httpClient, string token)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Authorization = new("Token", token);

        EndOfDay = new RestEndOfDayApi(_httpClient);
        Iex = new RestIexApi(_httpClient);
        Crypto = new RestCryptoApi(_httpClient);
        News = new RestNewsApi(_httpClient);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
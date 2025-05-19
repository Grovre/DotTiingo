using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Api;

internal class ApiResultFactory<T>(HttpClient httpClient)
{
    public async Task<T> CreateGet(HttpContent? content, string fullUrl)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
        req.Content = content;
        using var response = await httpClient.SendAsync(req);
#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync();
#endif
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<T>();
        return result
            ?? throw new Exception(
                $"Failed to deserialize response to {typeof(T).Name}");
    }
}

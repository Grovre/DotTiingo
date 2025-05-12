using DotTiingo.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo;

public class TiingoClient
{
    public ITiingoRestApi Rest { get; }

    public TiingoClient(HttpClient httpClient, string token)
    {
        Rest = new RestApi(httpClient, token);
    }
}

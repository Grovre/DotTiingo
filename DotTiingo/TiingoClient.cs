using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo;

public class TiingoClient
{
    public RestApi Rest { get; }

    public TiingoClient(HttpClient httpClient, string token)
    {
        Rest = new RestApi(httpClient, token);
    }
}

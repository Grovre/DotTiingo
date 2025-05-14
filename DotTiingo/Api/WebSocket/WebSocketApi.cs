using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Api.WebSocket;

public interface ITiingoWebSocketApi
{
    public ITiingoWebSocketCryptoApi Crypto { get; }
    public ITiingoWebSocketForexApi Forex { get; }
}

internal class WebSocketApi : ITiingoWebSocketApi
{
    public ITiingoWebSocketCryptoApi Crypto { get; }
    public ITiingoWebSocketForexApi Forex { get; }

    public WebSocketApi(string token)
    {
        Crypto = new WebSocketCryptoApi(token);
        Forex = new WebSocketForexApi(token);
    }
}

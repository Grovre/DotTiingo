using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Api;

/// <summary>
/// Helper class for Tiingo API base URLs.
/// </summary>
internal static class TiingoApiHelper
{
    /// <summary>
    /// The base URL for Tiingo REST API endpoints.
    /// </summary>
    public const string RestBaseUrl = "https://api.tiingo.com";
    /// <summary>
    /// The base URL for Tiingo WebSocket API endpoints.
    /// </summary>
    public const string WebSocketBaseUrl = "wss://api.tiingo.com";
}

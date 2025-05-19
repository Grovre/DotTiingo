using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.WebSocket.Response;

public record IexReferencePriceUpdate(
    DateTime Date,
    string Ticker,
    float ReferencePrice) : IResponseData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.WebSocket.Response;

public record UtilityResponse(
    char MessageType,
    int ResponseCode,
    string ResponseMessage,
    int? SubscriptionId) : AbstractResponse(MessageType);

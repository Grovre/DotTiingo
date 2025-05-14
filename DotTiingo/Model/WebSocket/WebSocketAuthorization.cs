using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotTiingo.Model.WebSocket;

[JsonConverter(typeof(WebSocketAuthorization.JsonConverter))]
internal record WebSocketAuthorization(
    string EventName,
    string Authorization,
    int ThresholdLevel)
{
    internal class JsonConverter : JsonConverter<WebSocketAuthorization>
    {
        public override WebSocketAuthorization? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, WebSocketAuthorization value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("eventName", value.EventName);
            writer.WriteString("authorization", value.Authorization);
            writer.WriteStartObject("eventData");
            writer.WriteNumber("thresholdLevel", value.ThresholdLevel);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}
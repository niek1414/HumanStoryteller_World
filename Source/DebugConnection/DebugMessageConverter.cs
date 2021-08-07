using System;
using HumanStoryteller.DebugConnection.Incoming;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Linq;
using HumanStoryteller.Util.Logging;

namespace HumanStoryteller.DebugConnection {
    public class DebugMessageConverter : JsonConverter {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Message);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonObject = JObject.Load(reader);
            IncomingMessage message;
            MessageType type;
            try {
                type = (MessageType) Enum.Parse(typeof(MessageType), jsonObject["Type"].Value<string>());
            } catch (Exception e) {
                Tell.Err("Unable to parse MessageType: ", e.Message, e);
                return null;
            }

            switch (type) {
                //--- OUTGOING
                case MessageType.Runners:
                    return OutgoingMessateFound();
                case MessageType.DataBanks:
                    return OutgoingMessateFound();
                case MessageType.LocationInfo:
                    return OutgoingMessateFound();
                //--- INCOMING
                case MessageType.ExecuteEvent:
                    message = new ExecuteEvent();
                    break;
                case MessageType.LoadStory:
                    message = new LoadStory();
                    break;
                case MessageType.UpdateDataBank:
                    message = new UpdateDataBank();
                    break;
                case MessageType.SendLocationInfo:
                    message = new SendLocationInfo();
                    break;
                default:
                    Parser.Parser.LogParseError("Type", type.ToString());
                    return null;
            }

            serializer.Populate(jsonObject.CreateReader(), message);
            return message;
        }

        private object OutgoingMessateFound() {
            Tell.Warn("Received a outgoing message type, ignoring..");
            return null;
        }
    }
}
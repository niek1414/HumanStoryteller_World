using System;
using System.Collections.Generic;
using HumanStoryteller.DebugConnection.Outgoing;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Converters;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.WebSocketsSharp;
using HumanStoryteller.WebSocketsSharp.Server;
using Verse;

namespace HumanStoryteller.DebugConnection {
    public class DebugWebSocket : WebSocketBehavior {
        private static bool CanSend() {
            return HumanStoryteller.CreatorTools;
        }

        public static void TryUpdateRunners() {
            if (!CanSend()) return;
            if (Current.Game == null || HumanStoryteller.StoryComponent == null || HumanStoryteller.IsNoStory) return;
            var current = new List<StoryEventNode>();
            var longCurrent = HumanStoryteller.StoryComponent.StoryArc.LongStoryController.CurrentNodes();
            if (longCurrent != null) {
                current.AddRange(longCurrent);
            }

            var shortCurrent = HumanStoryteller.StoryComponent.StoryArc.ShortStoryController.CurrentNodes();
            if (shortCurrent != null) {
                current.AddRange(shortCurrent);
            }

            SendObject(new Runners(current));
        }

        public static void TryUpdateDataBanks() {
            if (!CanSend()) return;
            if (Current.Game == null || HumanStoryteller.StoryComponent == null || HumanStoryteller.IsNoStory) return;
            SendObject(new DataBanks(HumanStoryteller.StoryComponent));
        }

        public static void TrySendLocationData(string locationString) {
            if (!CanSend()) return;
            if (Current.Game == null) return;
            SendObject(new LocationInfo(locationString));
        }

        protected override void OnMessage(MessageEventArgs e) {
            try {
                if (JsonConvert.DeserializeObject<Message>(e.Data, new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new List<JsonConverter> {new DebugMessageConverter(), new StringEnumConverter()}
                }) is IncomingMessage incomingMessage) {
                    incomingMessage.Handle();
                }
            } catch (Exception ex) {
                Tell.Err("Error in the OnMessage: " + ex.Message, ex);
            }
        }

        protected override void OnClose(CloseEventArgs e) {
            Tell.Log("A client disconnected from the debugger " + e.Reason);
            base.OnClose(e);
        }

        protected override void OnError(ErrorEventArgs e) {
            Tell.Err("A error in the debug communication: " + e.Message);
            base.OnError(e);
        }

        protected override void OnOpen() {
            Tell.Log("A client connected to the debugger");
            base.OnOpen();
            TryUpdateRunners();
        }

        private static void SendObject(object obj) {
            var messageJson = JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> {new StringEnumConverter()}
            });
            try {
                HumanStoryteller.DebugWebSocketConnection.WebSocketServices["/"].Sessions.BroadcastAsync(
                    messageJson,
                    () => Tell.Log("Send a message to the debug connection", messageJson)
                );
            } catch (InvalidOperationException e) {
                Tell.Warn("Unable to connect to the websocket, could be a permissions issue or a debugger is connected.");
                throw;
            }
        }
    }
}
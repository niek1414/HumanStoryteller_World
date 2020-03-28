namespace HumanStoryteller.DebugConnection.Outgoing {
    public class LocationInfo : Message {
        public string Value { get; set; }

        public LocationInfo() : base(MessageType.LocationInfo) {
        }

        public LocationInfo(string value) : this() {
            Value = value;
        }

        public override string ToString() {
            return $"Value: [{Value}]";
        }
    }
}
namespace HumanStoryteller.DebugConnection {
    public class Message {
        public MessageType Type { get; set; }
        
        public Message(MessageType type) {
            Type = type;
        }
    }
    
   
    public enum MessageType {
        Runners,
        LoadStory,
        DataBanks,
        UpdateDataBank,
        ExecuteEvent,
        SendLocationInfo,
        LocationInfo
    } 
}
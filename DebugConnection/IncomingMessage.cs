namespace HumanStoryteller.DebugConnection; 
public abstract class IncomingMessage : Message {
    protected IncomingMessage(MessageType type) : base(type) {
    }

    public abstract void Handle();
}
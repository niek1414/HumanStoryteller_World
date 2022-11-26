namespace HumanStoryteller.Parser; 
public class Connection {
    public long Offset { get; set; }
    public string Uuid { get; set; }

    public override string ToString() {
        return $"Offset: [{Offset}], Uuid: [{Uuid}]";
    }
}
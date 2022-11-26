using Verse;

namespace HumanStoryteller.Util.Overlay; 
public class Pointer : IOverlayItem {

    private enum State {
        Show,
        Hide
    }

    private State _currentState = State.Show;
    private Map _map;
    private IntVec3 _location;

    public Pointer() {
    }

    public Pointer(Map map, IntVec3 location) {
        _map = map;
        _location = location;
    }

    public bool Step() {
        return _currentState == State.Hide;
    }


    public void HighPrio() {
        if (Find.CurrentMap == _map) {
            GenDraw.DrawArrowPointingAt(_location.ToVector3Shifted());
        }
    }

    public void NotifyEnd() {
        SetState(State.Hide);
    }

    public bool ShouldBlockInput() {
        return false;
    }

    private void SetState(State state) {
        _currentState = state;
    }


    public void ExposeData() {
        Scribe_References.Look(ref _map, "map");
        Scribe_Values.Look(ref _location, "location");
        Scribe_Values.Look(ref _currentState, "currentState");
    }
}
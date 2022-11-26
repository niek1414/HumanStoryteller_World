using System;
using HumanStoryteller.Model.Incident;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter; 
public class LocationFilter : PawnGroupFilter {
    public const String Name = "Location";

    public Location Location;
    
    public LocationFilter() {
    }

    protected override bool Filter(Pawn p, Map map) {
        return Location.GetZone(map).Contains(p.Position);
    }
    
    protected override void ExposeFilter() {
        Scribe_Deep.Look(ref Location, "location");
    }

    public override string ToString() {
        return $"{base.ToString()}, Location: [{Location}]";
    }
}
using Verse;

namespace HumanStoryteller.Util.Overlay; 
public interface IRadioItem : IExposable {
    /**
     * Draws the item over other game components
     * Returns bool. If true the item should be removed from the draw loop.
     * Offset param defines heights of previous messages.
     * Should increase offset with own height.
     */
    bool Step(ref float offset);
}
using Verse;

namespace HumanStoryteller.Util.Overlay; 
public interface IOverlayItem : IExposable {
    /**
     * Draws the item over other game components
     * Returns bool. If true the item should be removed from the draw loop.
     */
    bool Step();
    
    void HighPrio();

    void NotifyEnd();

    bool ShouldBlockInput();
}
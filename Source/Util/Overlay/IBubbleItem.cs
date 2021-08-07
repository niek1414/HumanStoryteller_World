using Verse;

namespace HumanStoryteller.Util.Overlay {
    public interface IBubbleItem : IExposable {
        /**
         * Draws the item over other game components
         * Returns bool. If true the item should be removed from the draw loop.
         */
        bool Step();

        void OtherBubbleAdded(Pawn p);

        Pawn GetOwner();
    }
}
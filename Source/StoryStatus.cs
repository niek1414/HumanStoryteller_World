using RimWorld.Planet;
using Verse;

namespace HumanStoryteller {
    public class StoryStatus : IExposable {
        public bool DisableSpeedControls;
        public bool DisableCameraControls;
        public bool ShowWorld;
        public GlobalTargetInfo FollowThing = GlobalTargetInfo.Invalid;
        private int _forceSlowMotionUntil;
        public bool JumpException;
        public bool MovieMode;
        public bool CreatingStructure;
        
        public StoryStatus() {
        }

        public bool ForcedSlowMotion => Find.TickManager.TicksGame < _forceSlowMotionUntil;

        public void SignalForceSlowMotion() {
            _forceSlowMotionUntil = Find.TickManager.TicksGame + 150;
        }

        public void ExposeData() {
            //JumpException & CreatingStructure purposely ignored.
            Scribe_Values.Look(ref DisableSpeedControls, "disableSpeedControls");
            Scribe_Values.Look(ref DisableCameraControls, "disableCameraControls");
            Scribe_Values.Look(ref ShowWorld, "showWorld");
            Scribe_Values.Look(ref MovieMode, "movieMode");
            Scribe_TargetInfo.Look(ref FollowThing, "followThing");
            Scribe_Values.Look(ref _forceSlowMotionUntil, "forceSlowMotionUntil");
        }
    }
}
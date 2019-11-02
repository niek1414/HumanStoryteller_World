using System;
using Harmony;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ControlCamera : HumanIncidentWorker {
        public const String Name = "ControlCamera";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_ControlCamera)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_ControlCamera
                allParams = Tell.AssertNotNull((HumanIncidentParams_ControlCamera) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");
            
            var sc = HumanStoryteller.StoryComponent;
            sc.StoryStatus.FollowThing = GlobalTargetInfo.Invalid;
            
            if (allParams.ShowTile) {
                CameraJumper.TryShowWorld();
                Find.WorldCameraDriver.JumpTo(parms.Target.GetTileFromTarget());
            } else {
                if (allParams.Location.isSet()) {
                    CameraJumper.TryHideWorld();
                    if (allParams.Location.isPawn()) {
                        Pawn p = PawnUtil.GetPawnByName(allParams.Location.Value);
                        if (p == null) {
                            Tell.Warn($"Pawn with name {allParams.Location.Value} not found. Not jumping or locking camera.");
                            SendLetter(allParams);
                            return ir;
                        }
                        if (allParams.Follow) {
                            sc.StoryStatus.FollowThing = p;
                        }
                        CameraJumper.TryJump(new GlobalTargetInfo(p));
                    } else {
                        var map = (Map)allParams.GetTarget();
                        CameraJumper.TryJump(new GlobalTargetInfo(allParams.Location.GetSingleCell(map), map));
                    }
                }
            }

            var zoomValue = allParams.Zoom.GetValue();
            if (zoomValue != -1) {
                Traverse.Create(Find.CameraDriver).Field("desiredSize").SetValue(Mathf.Clamp(zoomValue, 11f, 60f));
            }

            sc.StoryStatus.DisableCameraControls = allParams.LockCamera;
            sc.StoryStatus.ShowWorld = Find.World.renderer.wantedMode == WorldRenderMode.Planet;

            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_ControlCamera : HumanIncidentParms {
        public Location Location = new Location();
        public Number Zoom = new Number();
        public bool LockCamera;
        public bool Follow;
        public bool ShowTile;

        public HumanIncidentParams_ControlCamera() {
        }

        public HumanIncidentParams_ControlCamera(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Location: [{Location}], Zoom: [{Zoom}], Follow: [{Follow}], ShowTile: [{ShowTile}], LockCamera: [{LockCamera}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Location, "location");
            Scribe_Deep.Look(ref Zoom, "zoom");
            Scribe_Values.Look(ref Follow, "follow");
            Scribe_Values.Look(ref ShowTile, "showTile");
            Scribe_Values.Look(ref LockCamera, "lockCamera");
        }
    }
}
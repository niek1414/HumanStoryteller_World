using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse.Sound;

namespace HumanStoryteller.Helper.SoundHelper {
    public class SoundGrain : AudioGrain {
        private readonly AudioClip _audioClip;

        public SoundGrain(AudioClip audioClip) {
            _audioClip = audioClip;
        }

        [DebuggerHidden]
        public override IEnumerable<ResolvedGrain> GetResolvedGrains() {
            yield return new ResolvedGrain_Clip(_audioClip);
        }
    }
}
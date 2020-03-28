using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using HarmonyLib;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Helper.SoundHelper;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Web;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using NAudio.Wave;
using RuntimeAudioClipLoader;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_PlayAudio : HumanIncidentWorker {
        public const String Name = "PlayAudio";

        private static object Lock = new object();
        private static String CachePath = (OSUtil.IsWindows ? "C:\\" : "/") +
                                          Path.Combine(Path.Combine("tmp", "RimWorld"), Path.Combine("HumanStoryteller", "audio"));

        private const string DriveDownloader = "https://drive.google.com/uc";
        private const string FreeSoundDownloader = "http://freesound.org/data/previews/";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_PlayAudio)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_PlayAudio
                allParams = Tell.AssertNotNull((HumanIncidentParams_PlayAudio) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");
            IncidentResult_Audio irAudio = new IncidentResult_Audio();

            var message = "Audio by " + allParams.Author;
            var id = FileToId(allParams.File, out var drive);
            var path = Path.Combine(CachePath,
                OSUtil.SaveFileName(id) + (id.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase) ? "" : ".mp3"));

            if (drive) {
                PlayFileOrConvert(path, irAudio, allParams, message);
            } else {
                PlayFileOrConvert(path, irAudio, allParams, message + " on FreeSound.org");
            }

            return irAudio;
        }

        public override void PreLoad(HumanIncidentParms parms) {
            if (!(parms is HumanIncidentParams_PlayAudio)) {
                Tell.Err("Tried to preload " + GetType() + " but param type was " + parms.GetType());
                return;
            }

            HumanIncidentParams_PlayAudio
                allParams = Tell.AssertNotNull((HumanIncidentParams_PlayAudio) parms, nameof(parms), GetType().Name);
            Tell.Log($"Preloading event {Name} with:{allParams}");

            Interlocked.Increment(ref HumanStoryteller.ConcurrentActions);

            var thread = new Thread(() => {
                try {
                    var id = FileToId(allParams.File, out var drive);
                    if (drive) {
                        DownloadAndPreloadFile(DriveDownloader + "?export=download&id=" + id, id);
                    } else {
                        DownloadAndPreloadFile(FreeSoundDownloader + id, id);
                    }
                    Interlocked.Decrement(ref HumanStoryteller.ConcurrentActions);
                } catch (Exception e) {
                    Tell.Err("Error in audio preload", e);
                }
            }) {Name = "Preload audio " + allParams.File};
            thread.Start();
        }

        private void DownloadAndPreloadFile(string url, string id) {
            try {
                Directory.CreateDirectory(CachePath);
                var tmpPath = Path.Combine(CachePath,
                    OSUtil.SaveFileName(id) + (id.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase) ? "" : ".mp3"));

                Client.DownloadFile(tmpPath, url);
                LoadAndConvertFile(tmpPath);
            } catch (Exception e) {
                Tell.Err("Exception in preloading audio " + url, e);
            }
        }

        private static void LoadAndConvertFile(string filePath) {
            if (!HasMp3Support() && ".mp3".EqualsIgnoreCase(Path.GetExtension(filePath))) {
                filePath = filePath.Remove(filePath.Length - 3) + "wav";
                if (!File.Exists(filePath)) {
                    ConvertWithAfConvert(filePath, filePath);
                }
            }

            void LoadClipWithManager() {
                try {
                    lock (Lock) {
                        Manager.Load(filePath);
                    }
                } catch (ArgumentNullException) {
                    Tell.Warn("Corrupt audio file found, removing from cache");
                    File.Delete(filePath);
                    throw;
                }
            }

            try {
                if (HasMp3Support()) {
                    LoadClipWithManager();
                } else {
                    filePath = GenFilePaths.SafeURIForUnityWWWFromPath(filePath);
                    var www = new WWW(filePath);

                    while (!www.isDone) {
                        Thread.Sleep(40);
                    }

                    if (www.error != null)
                        Tell.Err(www.error);
                    var audioClip = www.GetAudioClip(false, false, AudioType.UNKNOWN);
                    audioClip.LoadAudioData();
                    audioClip.name = Path.GetFileNameWithoutExtension(filePath);
                    ((Dictionary<string, AudioClip>) Traverse.Create(typeof(Manager)).Field("cache").GetValue()).Add(filePath, audioClip);
                }
            } catch (Exception e) {
                Tell.Err(e.Message, e);
            }
        }

        private static string FileToId(string file, out bool drive) {
            drive = false;
            var id = file;
            if (file.StartsWith("f__")) {
                id = file.Substring(3);
            } else if (file.StartsWith("d__")) {
                id = file.Substring(3);
                drive = true;
            }

            return id;
        }

        private static bool HasMp3Support() {
            void CheckFunc() {
                Mp3FileReader reader = new Mp3FileReader("");
            }

            try {
                CheckFunc();
            } catch (Exception e) {
                if (e.GetType() == typeof(TypeLoadException)) {
                    Tell.Warn("No MP3 support, converting to wav");
                    return false;
                }
            }

            return true;
        }

        private static void PlayFileOrConvert(string filePath, IncidentResult_Audio ir, HumanIncidentParams_PlayAudio allParams, string message) {
            if (!HasMp3Support() && ".mp3".EqualsIgnoreCase(Path.GetExtension(filePath))) {
                var wavPath = filePath.Remove(filePath.Length - 3) + "wav";
                if (!File.Exists(wavPath)) {
                    ConvertWithAfConvert(filePath, wavPath);
                }

                PlayFile(wavPath, ir, allParams, message);
            } else {
                PlayFile(filePath, ir, allParams, message);
            }
        }

        private static void ConvertWithAfConvert(string mp3Path, string wavPath) {
            Process p = null;
            try {
                var psi = new ProcessStartInfo {
                    FileName = "afconvert",
                    Arguments = $" -f WAVE -d LEI24 {mp3Path} {wavPath}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                p = Process.Start(psi);
                while (p != null && !p.HasExited) {
                    Thread.Sleep(10);
                }
            } finally {
                if (p != null && p.HasExited == false)
                    try {
                        p.Kill();
                    } catch (InvalidOperationException) {
                        //Ignore
                    }
            }
        }

        private static void PlayFile(string url, IncidentResult_Audio ir, HumanIncidentParams_PlayAudio allParams, string message) {
            try {
                AudioClip audioClip = Manager.Load(url);

                while (audioClip.loadState == AudioDataLoadState.Loading) {
                    Thread.Sleep(40);
                }

                if (audioClip.loadState != AudioDataLoadState.Loaded) {
                    Tell.Warn("Unable to load audio file: " + url);
                    return;
                }

                ir.EndAfter = audioClip.length + RealTime.LastRealTime;
                if (allParams.IsSong) {
                    var songDef = CreateSongDef(allParams, audioClip, url);

                    Find.MusicManagerPlay.ForceStartSong(songDef, false);
                } else {
                    var soundDef = CreateSoundDef(allParams, audioClip);
                    var resolvedGrains = soundDef.subSounds[0].grains[0].GetResolvedGrains();
                    ResolvedGrain_Clip resolvedGrainClip = resolvedGrains.First() as ResolvedGrain_Clip;

                    if (SampleOneShot.TryMakeAndPlay(soundDef.subSounds[0], resolvedGrainClip.clip, SoundInfo.OnCamera()) != null) {
                        SoundSlotManager.Notify_Played(soundDef.slot, resolvedGrainClip.clip.length);
                    }
                }
            } catch (Exception e) {
                Tell.Err(e.Message, e);
            }

            Messages.Message(message, MessageTypeDefOf.SilentInput);
        }

        private static SoundDef CreateSoundDef(HumanIncidentParams_PlayAudio allParams, AudioClip audioClip) {
            var o = SoundDefOf.TabOpen;
            var soundDef = new SoundDef {
                defName = "CustomSong_" + Guid.NewGuid(),
                label = "Audio by " + allParams.Author,
                debugRandomId = 7759,
                description = "Audio by " + allParams.Author,
                defPackage = o.defPackage,
                generated = o.generated,
                index = o.index,
                modExtensions = o.modExtensions,
                shortHash = o.shortHash,
                ignoreConfigErrors = o.ignoreConfigErrors,
                modContentPack = o.modContentPack,
                context = o.context,
                slot = string.Empty,
                sustain = o.sustain,
                eventNames = o.eventNames,
                isUndefined = o.isUndefined,
                maxSimultaneous = 10,
                maxVoices = 10,
                priorityMode = o.priorityMode,
                testSustainer = o.testSustainer,
                sustainFadeoutTime = o.sustainFadeoutTime,
                sustainStartSound = o.sustainStartSound,
                sustainStopSound = o.sustainStopSound
            };
            var volume = allParams.Volume.GetValue();
            soundDef.subSounds = new List<SubSoundDef> {
                new SubSoundDef {
                    grains = new List<AudioGrain> {new SoundGrain(audioClip)},
                    volumeRange = new FloatRange(volume * 100, volume * 100),
                    filters = o.subSounds[0].filters,
                    name = "subsound_" + Guid.NewGuid(),
                    distRange = o.subSounds[0].distRange,
                    onCamera = o.subSounds[0].onCamera,
                    paramMappings = o.subSounds[0].paramMappings,
                    parentDef = soundDef,
                    pitchRange = o.subSounds[0].pitchRange,
                    repeatMode = o.subSounds[0].repeatMode,
                    sustainAttack = o.subSounds[0].sustainAttack,
                    sustainLoop = o.subSounds[0].sustainLoop,
                    sustainRelease = o.subSounds[0].sustainRelease,
                    muteWhenPaused = true,
                    startDelayRange = o.subSounds[0].startDelayRange,
                    sustainIntervalRange = o.subSounds[0].sustainIntervalRange,
                    sustainLoopDurationRange = o.subSounds[0].sustainLoopDurationRange,
                    sustainSkipFirstAttack = o.subSounds[0].sustainSkipFirstAttack,
                    tempoAffectedByGameSpeed = o.subSounds[0].tempoAffectedByGameSpeed
                }
            };
            return soundDef;
        }

        private static SongDef CreateSongDef(HumanIncidentParams_PlayAudio allParams, AudioClip audioClip, string filePath) {
            SongDef o = SongDefOf.EntrySong;
            SongDef songDef = new SongDef {
                defName = "CustomSong_" + Guid.NewGuid(),
                label = "Audio by " + allParams.Author,
                volume = allParams.Volume.GetValue(),
                clip = audioClip,
                clipPath = filePath,
                debugRandomId = 7759,
                description = "Audio by " + allParams.Author,
                defPackage = o.defPackage,
                commonality = 0,
                generated = o.generated,
                index = 7759,
                tense = false,
                allowedSeasons = o.allowedSeasons,
                modExtensions = o.modExtensions,
                shortHash = 7759,
                ignoreConfigErrors = o.ignoreConfigErrors,
                modContentPack = o.modContentPack,
                playOnMap = o.playOnMap,
                allowedTimeOfDay = TimeOfDay.Any
            };
            return songDef;
        }
    }

    public class HumanIncidentParams_PlayAudio : HumanIncidentParms {
        public string File = "";
        public string Author = "";
        public bool IsSong;
        public Number Volume = new Number(1F);

        public HumanIncidentParams_PlayAudio() {
        }

        public HumanIncidentParams_PlayAudio(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, File: [{File}], Author: [{Author}], IsSong: [{IsSong}], Volume: [{Volume}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref File, "file");
            Scribe_Values.Look(ref Author, "author");
            Scribe_Values.Look(ref IsSong, "isSong");
            Scribe_Deep.Look(ref Volume, "volume");
        }
    }
}
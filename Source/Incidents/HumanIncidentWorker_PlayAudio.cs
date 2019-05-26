using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Web;
using RestSharp;
using RimWorld;
using RuntimeAudioClipLoader;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_PlayAudio : HumanIncidentWorker {
        public const String Name = "PlayAudio";

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

            string filePath;
            string message;

            if (allParams.File.StartsWith("s__")) {
                filePath = "/iframe-api/?t=" + allParams.File.Substring(3);
//                filePath = "https://api.soundcloud.com/tracks/" + allParams.File.Substring(3) + "/download?client_id=NmW1FlPaiL94ueEu7oziOWjYEzZzQDcK";
                message = "Audio by " + allParams.Author;
                RestClient restClient = new RestClient("http://soundclouddownloader.info") {
                    UserAgent = "HumanStoryteller"
                };
                RestRequest request = new RestRequest(filePath, Method.GET);

                restClient.ExecuteAsyncGet(request, (response, handle) => {
                    string downloadUrl;
                    try {
                        var urlStart = response.Content.IndexOf("/download.php", StringComparison.Ordinal);
                        var urlEnd = response.Content.IndexOf("\"", urlStart, StringComparison.Ordinal);
                        downloadUrl = response.Content.Substring(urlStart, urlEnd - urlStart);
                    } catch (Exception e) {
                        Tell.Err(e.Message, e);
                        return;
                    }

                    downloadFile("http://soundclouddownloader.info" + downloadUrl, irAudio, allParams, message, true, allParams.File.Substring(3));
                }, "GET");
            } else if (allParams.File.StartsWith("f__")) {
                filePath = "http://freesound.org/data/previews/" + allParams.File.Substring(3);
                message = "Audio by " + allParams.Author + " on FreeSound.org";
                downloadFile(filePath, irAudio, allParams, message);
            } else {
                filePath = "http://freesound.org/data/previews/" + allParams.File;
                message = "Audio by " + allParams.Author + " on FreeSound.org";
                downloadFile(filePath, irAudio, allParams, message);
            }

            return irAudio;
        }

        private void downloadFile(string url, IncidentResult_Audio ir, HumanIncidentParams_PlayAudio allParams, string message,
            bool soundCloud = false, string cloudId = "") {
            WebClient client = new WebClient();
            Tell.Log("Downloading audio form:" + url + (soundCloud ? " (cloudID: " + cloudId + ")": ""));
            client.DownloadDataAsync(new Uri(url));
            client.DownloadDataCompleted += (sender1, e1) => {
                try {
                    AudioClip audioClip;
                    MemoryStream stream = new MemoryStream(e1.Result);

                    if (soundCloud) {
                        audioClip = Manager.Load(stream, AudioFormat.mp3, cloudId);
                    } else {
                        audioClip = Manager.Load(stream, Manager.GetAudioFormat(url), url);
                    }

                    ir.EndAfter = audioClip.length + RealTime.LastRealTime;
                    if (allParams.IsSong) {
                        var songDef = CreateSongDef(allParams, audioClip, url);
                        Find.MusicManagerPlay.ForceStartSong(songDef, false);
                    } else {
                        var soundDef = CreateSoundDef(allParams, audioClip);
                        var resolvedGrains = soundDef.subSounds[0].grains[0].GetResolvedGrains();
                        ResolvedGrain_Clip resolvedGrainClip = resolvedGrains.First() as ResolvedGrain_Clip;
                        try {
                            if (SampleOneShot.TryMakeAndPlay(soundDef.subSounds[0], resolvedGrainClip.clip, SoundInfo.OnCamera()) != null) {
                                SoundSlotManager.Notify_Played(soundDef.slot, resolvedGrainClip.clip.length);
                            }
                        } catch (Exception e) {
                            Tell.Err(e.Message, e);
                        }
                    }

                    Messages.Message(message, MessageTypeDefOf.SilentInput);
                } catch (Exception e) {
                    Tell.Err("Exception in play audio:", e);
                }
            };
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
                    volumeRange = new FloatRange(volume * 30, volume * 30),
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
        public string File;
        public string Author;
        public bool IsSong;
        public Number Volume;

        public HumanIncidentParams_PlayAudio() {
        }

        public HumanIncidentParams_PlayAudio(String target, HumanLetter letter,
            Number volume, string file, string author, bool isSong) : base(target, letter) {
            File = file;
            Author = author;
            IsSong = isSong;
            Volume = volume;
        }

        public HumanIncidentParams_PlayAudio(string target, HumanLetter letter, string file = "", string author = "", bool isSong = false) : this(
            target, letter, new Number(1F), file, author, isSong) {
        }

        public override string ToString() {
            return $"{base.ToString()}, File: {File}, Author: {Author}, IsSong: {IsSong}, Volume: {Volume}";
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
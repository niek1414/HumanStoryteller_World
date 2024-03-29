using System.Collections.Generic;
using HarmonyLib;
using HumanStoryteller.Model.Action;
using Verse;

namespace HumanStoryteller.Model; 
public class StoryQueue : IExposable {
    public List<IStoryAction> Queue = new List<IStoryAction>();

    public void Add(IStoryAction action) {
        Queue.Add(action);
    }

    public void Tick() {
        if (Queue.Count > 0) {
            var local = Queue[0];
            local.Action();
            Queue.RemoveAt(0);
        }
    }

    public int Size() {
        return Queue.Count;
    }

    public void ExposeData() {
        Scribe_Collections.Look(ref Queue, "queue", LookMode.Deep);
    }

    public override string ToString() {
        return $"Queue: {Queue.Join()}";
    }
}
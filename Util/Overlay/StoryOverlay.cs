using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace HumanStoryteller.Util.Overlay; 
public class StoryOverlay : IExposable {
    private List<IOverlayItem> _items = new List<IOverlayItem>();
    private List<IRadioItem> _radio = new List<IRadioItem>();
    private List<IBubbleItem> _bubble = new List<IBubbleItem>();
    
    public StoryOverlay() {
    }

    public void AddItem(IOverlayItem item) {
        _items.Add(item);
    }

    public void AddRadio(IRadioItem item) {
        _radio.Insert(0, item);
    }

    public void AddBubble(IBubbleItem item) {
        _bubble.ForEach(i => i.OtherBubbleAdded(item.GetOwner()));
        _bubble.Insert(0, item);
    }

    public void NotifyEnd<T>() where T : IOverlayItem {
        _items.Where(item => item.GetType() == typeof(T)).ToList().ForEach(item => item.NotifyEnd());
    }
    
    public void DrawOverlay() {
        _items.Where(item => item.Step()).ToList().ForEach(item => _items.Remove(item));
        _bubble.Where(item => item.Step()).ToList().ForEach(item => _bubble.Remove(item));
        var offset = 20f + (HumanStoryteller.StoryComponent.StoryStatus.MovieMode ? MaxBarSize(_items.ToList()) * UI.screenHeight : 0);
        _radio.Where(t => t.Step(ref offset)).ToList().ForEach(item => _radio.Remove(item));
    }
    
    public void DrawHighPrio() {
        _items.ForEach(item => item.HighPrio());
    }

    public bool ShouldBlockInput() {
        return _items.Count > 0 && _items.Any(item => item.ShouldBlockInput());
    }
    
    private static float MaxBarSize(List<IOverlayItem> items) {
        float mostFound = 0;
        items.Where(i => i.GetType() == typeof(BlackBars)).ToList().ForEach(i => mostFound = Math.Max(((BlackBars) i).LastBarHeight, mostFound));
        return mostFound;
    }
    
    public override string ToString() {
        return $"Items: [{_items.Join(i => i.GetType() + ": [" + i + "]")}], Radio: [{_radio.Join(r => r.GetType() + ": [" + r + "]")}], Bubble: [{_bubble.Join(r => r.GetType() + ": [" + r + "]")}]";
    }
    
    public void ExposeData() {
        Scribe_Collections.Look(ref _items, "items", LookMode.Deep);
        Scribe_Collections.Look(ref _radio, "radio", LookMode.Deep);
        Scribe_Collections.Look(ref _bubble, "bubble", LookMode.Deep);
    }
}
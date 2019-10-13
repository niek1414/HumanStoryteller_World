using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HumanStoryteller.Util.Overlay {
    public class StoryOverlay : IExposable {
        private Dictionary<IOverlayItem, bool> _items = new Dictionary<IOverlayItem, bool>();
        private List<IRadioItem> _radio = new List<IRadioItem>();
        public StoryOverlay() {
        }

        public void AddItem(IOverlayItem item, bool shouldBlockInput = false) {
            _items.Add(item, shouldBlockInput);
        }

        public void AddRadio(IRadioItem item) {
            _radio.Insert(0, item);
        }

        public void NotifyEnd<T>() where T : IOverlayItem {
            _items.Where(item => item.Key.GetType() == typeof(T)).ToList().ForEach(item => item.Key.NotifyEnd());
        }
        
        public void DrawOverlay() {
            _items.Where(item => item.Key.Step()).ToList().ForEach(item => _items.Remove(item.Key));
            var offset = 20f + (HumanStoryteller.StoryComponent.StoryStatus.MovieMode ? MaxBarSize(_items.Keys.ToList()) * UI.screenHeight : 0);
            _radio.Where(t => t.Step(ref offset)).ToList().ForEach(item => _radio.Remove(item));
        }

        public bool ShouldBlockInput() {
            return _items.Count > 0 && _items.Any(item => item.Value);
        }
        
        private static float MaxBarSize(List<IOverlayItem> items) {
            float mostFound = 0;
            items.Where(i => i.GetType() == typeof(BlackBars)).ToList().ForEach(i => mostFound = Math.Max(((BlackBars) i).LastBarHeight, mostFound));
            return mostFound;
        }
        
        public void ExposeData() {
            Scribe_Collections.Look(ref _items, "items", LookMode.Deep);
            Scribe_Collections.Look(ref _radio, "radio", LookMode.Deep);
        }
    }
}
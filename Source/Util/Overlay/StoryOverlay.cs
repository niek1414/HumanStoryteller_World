using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HumanStoryteller.Util.Overlay {
    public class StoryOverlay : IExposable {
        private List<IOverlayItem> _items = new List<IOverlayItem>();
        private List<IRadioItem> _radio = new List<IRadioItem>();
        public StoryOverlay() {
        }

        public void AddItem(IOverlayItem item) {
            _items.Add(item);
        }

        public void AddRadio(IRadioItem item) {
            _radio.Add(item);
        }

        public void DrawOverlay() {
            _items.Where(item => item.Step()).ToList().ForEach(item => _items.Remove(item));
            var offset = 20f + (HumanStoryteller.StoryComponent.StoryStatus.MovieMode ? MaxBarSize(_items) * UI.screenHeight : 0);
            _radio.Where(t => t.Step(ref offset)).ToList().ForEach(item => _radio.Remove(item));
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
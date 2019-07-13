using System;
using System.Collections.Generic;

namespace HumanStoryteller {
    public class StoryQueue {
        private readonly Queue<Action> _queue = new Queue<Action>();
        
        public void Add(Action action) {
            _queue.Enqueue(action);
        }

        public void Tick() {
            if (_queue.Count > 0) {
                _queue.Dequeue()();
            }
        }

        public int Size() {
            return _queue.Count;
        }
    }
}
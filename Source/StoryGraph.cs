using System;
using System.Collections.Generic;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using RimWorld;
using Verse;

namespace HumanStoryteller {
    public class StoryGraph : IExposable {
        private StoryNode _root;
        private int _lastTransition;

        public StoryGraph() {
        }

        public StoryGraph(StoryNode root) {
            _root = root;
        }

        public StoryNode TryNewEvent(StoryNode current, int bigTick) {
            int tickPassed = bigTick - _lastTransition;
            if (current == null
                || current.LeftChild == null && current.RightChild == null
                || current.LeftChild != null && current.LeftChild.Offset > tickPassed
                                             && current.RightChild != null && current.RightChild.Offset > tickPassed) {
                return null;
            }

            Connection next;
            List<CheckCondition> conditions = current.Conditions;

            if (conditions != null) {
                bool allTrue = true;
                foreach (var condition in conditions) {
                    if (condition.Check(current)) continue;
                    allTrue = false;
                    break;
                }
                next = allTrue ? current.LeftChild : current.RightChild;
            } else {
                next = current.LeftChild;
            }

            if (next == null) {
                return null;
            }

            if (next.Offset <= tickPassed) {
                _lastTransition = bigTick;
                return next.Node;
            }

            return null;
        }

        public StoryNode GetCurrentNode(String uuid) {
            if (uuid == null) return null;
            return findNodeById(_root, uuid);
        }

        private StoryNode findNodeById(StoryNode current, String uuid) {
            if (current == null) {
                return null;
            }

            if (current.StoryEvent.Uuid.Equals(uuid)) {
                return current;
            }

            StoryNode leftAnswer = findNodeById(current.LeftChild?.Node, uuid);
            if (leftAnswer != null) {
                return leftAnswer;
            }

            return findNodeById(current.RightChild?.Node, uuid);
        }

        public StoryNode Root => _root;

        public override string ToString() {
            return $"Root: {_root}, LastTransition: {_lastTransition}";
        }
        
        public void ExposeData() {
            Scribe_Deep.Look(ref _root, "storyNode");
            Scribe_Values.Look(ref _lastTransition, "lastTransaction");
        }
    }
}
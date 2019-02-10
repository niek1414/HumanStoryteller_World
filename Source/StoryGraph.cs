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

        public StoryNode TryNewEvent(StoryEventNode current, int bigTick) {
            int tickPassed = bigTick - _lastTransition;
            var storyNode = current?.StoryNode;
            if (current?.StoryNode == null
                || storyNode.LeftChild == null && storyNode.RightChild == null
                || storyNode.LeftChild != null && storyNode.LeftChild.Offset > tickPassed
                                             && storyNode.RightChild != null && storyNode.RightChild.Offset > tickPassed) {
                return null;
            }

            Connection next;
            List<CheckCondition> conditions = storyNode.Conditions;

            if (conditions != null) {
                bool allTrue = true;
                foreach (var condition in conditions) {
                    if (condition.Check(current.Result)) continue;
                    allTrue = false;
                    break;
                }
                next = allTrue ? storyNode.LeftChild : storyNode.RightChild;
            } else {
                next = storyNode.LeftChild;
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
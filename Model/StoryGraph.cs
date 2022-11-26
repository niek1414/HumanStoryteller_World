using System;
using System.Collections.Generic;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Model; 
public class StoryGraph : IExposable {
    [JsonIgnore] public bool MissedLastIncidentCheck = true; //Start graph with a first check
    [JsonIgnore] public int ConsecutiveEventCounter = 0;
    [JsonIgnore] public List<StoryEventNode> CurrentNodes = new List<StoryEventNode>();
    [JsonIgnore] private List<StoryNode> _allNodes;
    private string _id;
    private StoryNode _root;

    public StoryGraph() {
    }

    public StoryGraph(string id, StoryNode root) {
        _id = id;
        _root = root;
        _allNodes = GetAllNodes();
    }

    public virtual bool IsShortStory() {
        return false;
    }

    public virtual bool IsLongStory() {
        return false;
    }

    public int CurrentNodeCount() {
        return CurrentNodes.Count;
    }

    public StoryNode TryNewEvent(StoryEventNode current, int tickPassed) {
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
            for (var i = 0; i < conditions.Count; i++) {
                var condition = conditions[i];
                if (condition.Check(current.Result, i)) continue;
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
            return next.Node;
        }

        return null;
    }

    public StoryNode GetCurrentNode(String uuid) {
        if (uuid == null) return null;
        return findNodeById(_root, uuid, new List<string>());
    }

    private StoryNode findNodeById(StoryNode current, String uuid, List<string> processed) {
        if (current == null) {
            return null;
        }

        string currentUuid = current.StoryEvent.Uuid;

        if (currentUuid.Equals(uuid)) {
            return current;
        }

        if (processed.Contains(currentUuid)) {
            return null; //Avoid infinite loops
        }

        processed.Add(currentUuid);

        StoryNode leftAnswer = findNodeById(current.LeftChild?.Node, uuid, processed);
        if (leftAnswer != null) {
            return leftAnswer;
        }

        return findNodeById(current.RightChild?.Node, uuid, processed);
    }

    public List<StoryNode> GetAllNodes() {
        return _allNodes ?? (_allNodes = GetAllNodes(_root, new List<StoryNode>()));
    }

    private List<StoryNode> GetAllNodes(StoryNode current, List<StoryNode> storyNodes) {
        if (current == null) {
            return storyNodes;
        }

        if (storyNodes.Contains(current)) {
            return storyNodes;
        }

        storyNodes.Add(current);

        GetAllNodes(current.LeftChild?.Node, storyNodes);
        return GetAllNodes(current.RightChild?.Node, storyNodes);
    }

    /**
     * Not thread-safe
     */
    public void UpdateCurrentNodes(List<StoryEventNode> oldCurrentNodes) {
        for (var i = 0; i < oldCurrentNodes.Count; i++) {
            var foundNode =
                GetCurrentNode(oldCurrentNodes[i]?.StoryNode.StoryEvent.Uuid);
            CurrentNodes.Add(foundNode == null
                ? null
                : new StoryEventNode(foundNode, oldCurrentNodes[i].ExecuteTick, oldCurrentNodes[i].Result));
            if (foundNode == null) {
                Tell.Warn("No current node found for previous current node with UUID: ", oldCurrentNodes[i]?.StoryNode.StoryEvent.Uuid);
            }
        }

        CurrentNodes.RemoveAll(item => item == null);
    }

    public void ResetGraph() {
        MissedLastIncidentCheck = true;
        ConsecutiveEventCounter = 0;
        CurrentNodes.Clear();
        CurrentNodes.Add(new StoryEventNode(Root, Find.TickManager.TicksGame / 600));
    }

    public StoryNode Root => _root;

    public override string ToString() {
        return
            $"Root: [{_root}], CurrentNodes: [{CurrentNodes.ToStringSafeEnumerable()}], ConsecutiveEventCounter: [{ConsecutiveEventCounter}], MissedLastIncidentCheck: [{MissedLastIncidentCheck}]";
    }

    public virtual void ExposeData() {
        Scribe_References.Look(ref _root, "storyNode");
        Scribe_Collections.Look(ref CurrentNodes, "currentNode", LookMode.Deep);
        Scribe_Collections.Look(ref _allNodes, "allNodes", LookMode.Deep);
    }
}
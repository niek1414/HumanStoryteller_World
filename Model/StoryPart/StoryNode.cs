﻿using System.Collections.Generic;
using HarmonyLib;
using HumanStoryteller.CheckConditions;
 using HumanStoryteller.Model.Incident;
 using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Model.StoryPart; 
public class StoryNode : IExposable, ILoadReferenceable {
    private StoryEvent _storyEvent;
    private Connection _leftChild;
    private Connection _rightChild;
    private List<CheckCondition> _conditions;
    private List<VariableModifications> _modifications;
    public bool Divider;

    public StoryNode() {
    }

    public StoryNode(StoryEvent storyEvent, Connection leftChild = null, Connection rightChild = null,
        List<CheckCondition> conditions = null, List<VariableModifications> modifications = null) {
        _storyEvent = Tell.AssertNotNull(storyEvent, nameof(storyEvent), GetType().Name);
        _leftChild = leftChild;
        _rightChild = rightChild;
        _conditions = conditions;
        _modifications = modifications;
    }

    public StoryEvent StoryEvent => _storyEvent;

    public Connection LeftChild {
        get => _leftChild;
        set => _leftChild = value;
    }

    public Connection RightChild {
        get => _rightChild;
        set => _rightChild = value;
    }

    public List<CheckCondition> Conditions {
        get => _conditions;
        set => _conditions = value;
    }

    public List<VariableModifications> Modifications {
        get => _modifications;
        set => _modifications = value;
    }

    public override string ToString() {
        return $"StoryEvent: [{_storyEvent}], LeftChild: [{_leftChild}], RightChild: [{_rightChild}], Conditions: [{_conditions.Join()}], Modifications: [{_modifications.Join()}], Divider: [{Divider}]";
    }

    public string GetUniqueLoadID() {
        return "HS_SN_" + _storyEvent.Uuid;
    }

    public void ExposeData() {
        Scribe_Deep.Look(ref _storyEvent, "storyEvent");
        Scribe_Deep.Look(ref _leftChild, "leftChild");
        Scribe_Deep.Look(ref _rightChild, "rightChild");
        Scribe_Values.Look(ref Divider, "divider");
        Scribe_Collections.Look(ref _conditions, "conditions", LookMode.Deep);
        Scribe_Collections.Look(ref _modifications, "modifications", LookMode.Deep);
    }
}
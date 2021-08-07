using System.Collections.Generic;
using HumanStoryteller.DebugConnection;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Util {
    public class StoryGraphWalkerUtil {
        public static HumanIncidentParams_Exit TickGraph(StoryGraph graph, int interval) {
            if (Find.TickManager.TicksGame % interval == 0) {
                graph.ConsecutiveEventCounter = 0;
                return IncidentLoop();
            } else if (graph.MissedLastIncidentCheck && graph.ConsecutiveEventCounter <= 10) {
                graph.MissedLastIncidentCheck = false;
                return IncidentLoop();
            }

            return null;

            HumanIncidentParams_Exit IncidentLoop() {
                foreach (StoryEventNode sen in MakeIntervalIncidents(graph)) {
                    if (sen?.StoryNode?.StoryEvent?.Incident?.Worker != null) {
                        var incident = sen.StoryNode.StoryEvent.Incident;
                        if (incident.Params is HumanIncidentParams_Exit exitParams) {
                            return exitParams;
                        }

                        sen.Result = incident.Worker.ExecuteIncident(incident.Params);
                        DataBankUtil.ProcessVariableModifications(sen.StoryNode.Modifications);
                    } else {
                        Tell.Warn("Returned a incident that was not defined");
                    }

                    DebugWebSocket.TryUpdateRunners();
                }
                return null;
            }
        }

        public static void TryExecuteNode(StoryGraph graph, string uuid, bool createRunner) {
            var sn = graph.GetCurrentNode(uuid);
            if (sn == null) return;

            IncidentResult incidentResult = null;
            if (sn.StoryEvent?.Incident?.Worker != null) {
                var incident = sn.StoryEvent.Incident;
                incidentResult = incident.Worker.ExecuteIncident(incident.Params);
                DataBankUtil.ProcessVariableModifications(sn.Modifications);
            } else {
                Tell.Warn("Unable to execute node with uuid " + uuid + ", the incident that was not defined");
            }

            if (createRunner) {
                graph.CurrentNodes.Add(new StoryEventNode(sn, Find.TickManager.TicksGame / 600, incidentResult));
                DebugWebSocket.TryUpdateRunners();
            }
        }

        private static IEnumerable<StoryEventNode> MakeIntervalIncidents(StoryGraph graph) {
            if (HumanStoryteller.InitiateEventUnsafe) {
                graph.MissedLastIncidentCheck = true;
                yield break;
            }

            graph.CurrentNodes.RemoveAll(item => item == null);
            if (graph.CurrentNodes.Count > 500) {
                Tell.Warn("More concurrent lanes then 500, probably unintentionally created by" +
                          " looping over a divider or merging multiple concurrent lanes.\n" +
                          "Unused lanes will be cleared! This means that if the story is updated, you may not be able to continue where you left off if you finished the story before.");
            } else if (graph.CurrentNodes.Count > 150) {
                Tell.Warn("More concurrent lanes then 150, this can hurt performance badly. This is because the storymaker used to much dividers.");
            }

            for (var i = 0; i < graph.CurrentNodes.Count; i++) {
                if (graph.ConsecutiveEventCounter > 10) {
                    yield break;
                }

                StoryEventNode currentNode = graph.CurrentNodes[i];
                if (currentNode == null) {
                    continue;
                }

                StoryNode sn = currentNode.StoryNode;

                if (i > 500) {
                    Tell.Err("Limiting lane check, stopped after 500 lanes. Last node: " + currentNode);
                    break;
                }

                if (sn.Divider) {
                    var left = sn.LeftChild != null ? new StoryEventNode(sn.LeftChild?.Node, StorytellerComp_HumanThreatCycle.IP) : null;
                    var right = sn.RightChild != null ? new StoryEventNode(sn.RightChild?.Node, StorytellerComp_HumanThreatCycle.IP) : null;
                    graph.CurrentNodes.Add(left);
                    graph.CurrentNodes[i] = right;
                    graph.MissedLastIncidentCheck = true;
                    yield return left;
                    graph.MissedLastIncidentCheck = true;
                    graph.ConsecutiveEventCounter += 2; //Always execute a divider's children together
                    yield return right;
                } else {
                    if (graph.CurrentNodes.Count > 150 && sn.LeftChild == null && sn.RightChild == null) {
                        graph.CurrentNodes[i] = null;
                    } else {
                        StoryNode newEvent = graph.TryNewEvent(currentNode, StorytellerComp_HumanThreatCycle.IP - currentNode.ExecuteTick);
                        if (newEvent == null) continue;
                        if (!newEvent.StoryEvent.Uuid.Equals(currentNode.StoryNode.StoryEvent.Uuid)) {
                            graph.MissedLastIncidentCheck = true;
                            graph.ConsecutiveEventCounter++;
                        }

                        graph.CurrentNodes[i] = new StoryEventNode(newEvent, StorytellerComp_HumanThreatCycle.IP);
                        yield return graph.CurrentNodes[i];
                    }
                }
            }

            graph.CurrentNodes.RemoveAll(item => item == null);
        }
    }
}
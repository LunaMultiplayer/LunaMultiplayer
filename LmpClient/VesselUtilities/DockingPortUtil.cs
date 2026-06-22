using System.Collections.Generic;
using System.Linq;

namespace LmpClient.VesselUtilities
{
    /// <summary>
    /// Utility for validating and recovering docking port FSM states.
    /// Inspired by DockRotate's DockingStateChecker and KML's KmlPartDock_Repair.
    /// </summary>
    public static class DockingPortUtil
    {
        /// <summary>
        /// FSM states that indicate the port is in a stable docked configuration.
        /// </summary>
        private static readonly string[] DockedStates =
        {
            "Docked (docker)",
            "Docked (dockee)",
            "Docked (same vessel)",
            "PreAttached"
        };

        /// <summary>
        /// Transient FSM states that may occur due to partial operations or proto reloads.
        /// These can potentially be recovered by resetting the FSM.
        /// </summary>
        private static readonly string[] RecoverableTransientStates =
        {
            "Disengage",
            "Acquire",
            "Acquire (dockee)"
        };

        /// <summary>
        /// Check if a docking port FSM is in a valid docked state.
        /// </summary>
        public static bool IsInDockedState(ModuleDockingNode node)
        {
            if (node?.fsm == null) return false;
            var state = node.fsm.currentStateName;
            return !string.IsNullOrEmpty(state) && DockedStates.Contains(state);
        }

        /// <summary>
        /// Check if a docking port FSM is in a recoverable transient state
        /// (stuck mid-transition due to timing/proto reload issues).
        /// </summary>
        public static bool IsInRecoverableTransientState(ModuleDockingNode node)
        {
            if (node?.fsm == null) return false;
            var state = node.fsm.currentStateName;
            return !string.IsNullOrEmpty(state) && RecoverableTransientStates.Contains(state);
        }

        /// <summary>
        /// Find the partner ModuleDockingNode by walking the part tree's attach nodes.
        /// Returns the partner node if this port's reference attach node connects to
        /// another part that has a ModuleDockingNode facing back at us, or null.
        /// </summary>
        public static ModuleDockingNode FindPartnerFromPartTree(ModuleDockingNode node)
        {
            if (node?.part == null) return null;

            var referenceNodeName = node.referenceAttachNode;
            if (string.IsNullOrEmpty(referenceNodeName)) return null;

            // Method 1: Direct attach node lookup
            var attachNode = node.part.FindAttachNode(referenceNodeName);
            if (attachNode?.attachedPart != null)
            {
                var otherDockingNodes = attachNode.attachedPart.FindModulesImplementing<ModuleDockingNode>();
                if (otherDockingNodes != null && otherDockingNodes.Count > 0)
                {
                    foreach (var other in otherDockingNodes)
                    {
                        if (string.IsNullOrEmpty(other.referenceAttachNode)) continue;
                        var otherAttach = other.part.FindAttachNode(other.referenceAttachNode);
                        if (otherAttach?.attachedPart == node.part)
                            return other;
                    }
                    return otherDockingNodes[0];
                }
            }

            // Method 2: Parent/child walk for docking connections.
            // When two ports dock, one becomes a child of the other.
            // The docking attachment goes through the referenceAttachNode (top),
            // while regular ship connections go through bottom or srfAttach.

            if (node.part.parent != null)
            {
                var parentDocks = node.part.parent.FindModulesImplementing<ModuleDockingNode>();
                if (parentDocks != null && parentDocks.Count > 0
                    && !IsBottomOrSurfaceAttachedTo(node.part, node.part.parent))
                {
                    return parentDocks[0];
                }
            }

            if (node.part.children != null)
            {
                foreach (var child in node.part.children)
                {
                    if (child == null) continue;
                    var childDocks = child.FindModulesImplementing<ModuleDockingNode>();
                    if (childDocks != null && childDocks.Count > 0
                        && !IsBottomOrSurfaceAttachedTo(child, node.part))
                    {
                        return childDocks[0];
                    }
                }
            }

            return null;
        }

        private static bool IsBottomOrSurfaceAttachedTo(Part part, Part other)
        {
            if (part.srfAttachNode != null && part.srfAttachNode.attachedPart == other)
                return true;
            var bottomNode = part.FindAttachNode("bottom");
            if (bottomNode != null && bottomNode.attachedPart == other)
                return true;
            return false;
        }

        /// <summary>
        /// Find the partner ModuleDockingNode by searching for a part matching
        /// the given dockedPartUId.  When <paramref name="sameVesselOnly"/> is
        /// provided, only that vessel's parts are searched — this prevents
        /// stale cross-vessel references (left over after undock) from causing
        /// false-positive recovery.
        /// </summary>
        public static ModuleDockingNode FindPartnerByUId(uint dockedPartUId, Vessel sameVesselOnly = null)
        {
            if (dockedPartUId == 0) return null;

            if (sameVesselOnly != null)
            {
                if (sameVesselOnly.parts == null) return null;
                foreach (var part in sameVesselOnly.parts)
                {
                    if (part.flightID == dockedPartUId)
                        return part.FindModulesImplementing<ModuleDockingNode>()?.FirstOrDefault();
                }
                return null;
            }

            foreach (var vessel in FlightGlobals.VesselsLoaded)
            {
                if (vessel?.parts == null) continue;
                foreach (var part in vessel.parts)
                {
                    if (part.flightID == dockedPartUId)
                    {
                        return part.FindModulesImplementing<ModuleDockingNode>()?.FirstOrDefault();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determine docker vs dockee roles for a pair. The child in the part tree
        /// is typically the dockee.
        /// </summary>
        private static void InferDockerDockeeRoles(ModuleDockingNode node, ModuleDockingNode partner,
            out string nodeState, out string partnerState)
        {
            // Check serialized state first — it's often still correct even when FSM is wrong
            if (!string.IsNullOrEmpty(node.state) && node.state.StartsWith("Docked"))
            {
                nodeState = node.state;
                partnerState = (nodeState == "Docked (docker)") ? "Docked (dockee)" : "Docked (docker)";
                return;
            }
            if (!string.IsNullOrEmpty(partner.state) && partner.state.StartsWith("Docked"))
            {
                partnerState = partner.state;
                nodeState = (partnerState == "Docked (docker)") ? "Docked (dockee)" : "Docked (docker)";
                return;
            }

            // Fall back to part tree hierarchy: parent = docker, child = dockee
            if (node.part?.parent == partner.part)
            {
                nodeState = "Docked (dockee)";
                partnerState = "Docked (docker)";
            }
            else
            {
                nodeState = "Docked (docker)";
                partnerState = "Docked (dockee)";
            }
        }

        /// <summary>
        /// Set up the cross-references between two docking nodes and force their FSMs
        /// to the correct docked states. This must be done BEFORE calling StartFSM
        /// because the "Docked (docker)" OnEnter callback accesses otherNode.
        /// </summary>
        private static void RecoverDockedPair(ModuleDockingNode node, ModuleDockingNode partner,
            string nodeState, string partnerState, string reason, Vessel vessel)
        {
            LunaLog.Log($"[LMP]: Recovering docked pair on {vessel.vesselName}: " +
                $"{node.part?.partName}({node.part?.flightID}) → '{nodeState}', " +
                $"{partner.part?.partName}({partner.part?.flightID}) → '{partnerState}' " +
                $"[{reason}]");

            // Set up cross-references BEFORE forcing FSM (OnEnter accesses otherNode)
            node.otherNode = partner;
            partner.otherNode = node;

            // Ensure dockedPartUId is set on both sides
            if (node.dockedPartUId == 0 && partner.part != null)
                node.dockedPartUId = partner.part.flightID;
            if (partner.dockedPartUId == 0 && node.part != null)
                partner.dockedPartUId = node.part.flightID;

            // Force the FSM states — the docker side first since dockee's OnEnter
            // may reference the docker
            var dockerNode = (nodeState == "Docked (docker)") ? node : partner;
            var dockeeNode = (nodeState == "Docked (docker)") ? partner : node;
            var dockerState = (nodeState == "Docked (docker)") ? nodeState : partnerState;
            var dockeeState = (nodeState == "Docked (docker)") ? partnerState : nodeState;

            if (!IsInDockedState(dockerNode))
            {
                dockerNode.fsm.StartFSM(dockerState);
                LunaLog.Log($"[LMP]: Docker {dockerNode.part?.partName}({dockerNode.part?.flightID}) " +
                    $"FSM → '{dockerNode.fsm.currentStateName}'");
            }

            if (!IsInDockedState(dockeeNode))
            {
                dockeeNode.fsm.StartFSM(dockeeState);
                LunaLog.Log($"[LMP]: Dockee {dockeeNode.part?.partName}({dockeeNode.part?.flightID}) " +
                    $"FSM → '{dockeeNode.fsm.currentStateName}'");
            }
        }

        /// <summary>
        /// Determine the most likely docked state for a port that needs recovery before undocking.
        /// Used by VesselUndock for the remote-undock path only.
        /// </summary>
        public static string InferDockedStateForUndock(ModuleDockingNode node)
        {
            if (!string.IsNullOrEmpty(node.state) && node.state.StartsWith("Docked"))
                return node.state;

            if (node.otherNode?.fsm != null)
            {
                var otherState = node.otherNode.fsm.currentStateName;
                if (otherState == "Docked (dockee)" || otherState == "Docked (same vessel)")
                    return "Docked (docker)";
                if (otherState == "Docked (docker)")
                    return "Docked (dockee)";
            }

            return "Docked (docker)";
        }

        /// <summary>
        /// Ensure the docking node is in a state where an undock call is valid.
        /// If the node is stuck in a recoverable transient state, this method
        /// attempts to recover it to a docked state before undock proceeds.
        /// </summary>
        public static bool EnsureRecoverableForUndock(ModuleDockingNode node, string context, out string failureReason)
        {
            failureReason = null;

            if (node?.fsm == null)
            {
                failureReason = "docking node or FSM is null";
                return false;
            }

            if (IsInDockedState(node))
                return true;

            if (IsInRecoverableTransientState(node))
            {
                var targetState = InferDockedStateForUndock(node);
                if (TryRecoverToDockedState(node, targetState))
                    return true;

                failureReason =
                    $"failed transient recovery in {context}: '{node.fsm.currentStateName}' -> '{targetState}'";
                return false;
            }

            // Fallback: when FSM reports an unexpected state, try to rehydrate the docking pair
            // from part-tree / dockedPartUId data before hard-blocking undock.
            var partner = FindPartnerFromPartTree(node);
            if (partner == null && node.dockedPartUId != 0)
                partner = FindPartnerByUId(node.dockedPartUId, node.vessel) ?? FindPartnerByUId(node.dockedPartUId);

            if (partner?.fsm != null)
            {
                string nodeState, partnerState;
                InferDockerDockeeRoles(node, partner, out nodeState, out partnerState);
                RecoverDockedPair(node, partner, nodeState, partnerState,
                    $"fallback pre-undock recovery from fsm='{node.fsm.currentStateName}'", node.vessel);

                if (IsInDockedState(node))
                    return true;
            }

            var inferredState = InferDockedStateForUndock(node);
            if (TryRecoverToDockedState(node, inferredState))
                return true;

            failureReason =
                $"unsupported undock state in {context}: '{node.fsm.currentStateName}'";
            return false;
        }

        /// <summary>
        /// Attempt to recover a docking port for the remote-undock path (VesselUndock.cs).
        /// Sets up otherNode before forcing FSM to avoid NullRef.
        /// </summary>
        public static bool TryRecoverToDockedState(ModuleDockingNode node, string targetState)
        {
            if (node?.fsm == null) return false;

            var currentState = node.fsm.currentStateName;
            LunaLog.Log($"[LMP]: Attempting docking port FSM recovery: '{currentState}' → '{targetState}' " +
                $"on part {node.part?.partName} (flightID: {node.part?.flightID})");

            // Find and set otherNode before StartFSM — the OnEnter callback accesses it
            if (node.otherNode == null)
            {
                var partner = FindPartnerFromPartTree(node);
                if (partner == null && node.dockedPartUId != 0)
                    partner = FindPartnerByUId(node.dockedPartUId);
                if (partner != null)
                {
                    node.otherNode = partner;
                    partner.otherNode = node;
                }
            }

            node.fsm.StartFSM(targetState);

            var newState = node.fsm.currentStateName;
            if (newState == targetState)
            {
                LunaLog.Log($"[LMP]: Docking port FSM recovered to '{targetState}'");
                return true;
            }

            LunaLog.LogWarning($"[LMP]: Docking port FSM recovery failed — state is '{newState}' " +
                $"after StartFSM('{targetState}')");
            return false;
        }

        /// <summary>
        /// Check all docking ports on a vessel and fix any whose FSM state doesn't match
        /// their actual docked configuration. Called when a vessel goes off rails (unpacks).
        ///
        /// Detection cascade:
        ///   1. Serialized state = Docked but FSM wrong
        ///   2. dockedPartUId set but FSM not docked
        ///   3. Part tree shows physical docking partner but FSM not docked
        ///   4. Stuck in transient state with no partner → reset to Ready
        ///
        /// For cases 1-3, we recover BOTH sides of the pair together, setting up
        /// cross-references (otherNode, dockedPartUId) before forcing FSM states.
        /// </summary>
        public static void FixDockingPortFsmStates(Vessel vessel)
        {
            if (vessel == null || !vessel.loaded) return;

            List<ModuleDockingNode> dockingNodes;
            try
            {
                dockingNodes = vessel.FindPartModulesImplementing<ModuleDockingNode>();
            }
            catch
            {
                return;
            }

            if (dockingNodes == null || dockingNodes.Count == 0) return;

            foreach (var node in dockingNodes)
            {
                if (node?.fsm == null) continue;

                // Some broken ports are stuck in a docked FSM state even though
                // they have no physical partner (ghost "Undock" button).
                // Validate docked states before skipping.
                if (IsInDockedState(node))
                {
                    var dockedPartner = FindPartnerFromPartTree(node);

                    if (dockedPartner != null)
                    {
                        if (node.otherNode != dockedPartner)
                        {
                            LunaLog.Log($"[LMP]: Setting otherNode on {vessel.vesselName}" +
                                $" part {node.part?.partName}({node.part?.flightID})" +
                                $" -> partner {dockedPartner.part?.partName}({dockedPartner.part?.flightID})");
                            node.otherNode = dockedPartner;
                        }
                        if (dockedPartner.otherNode != node)
                        {
                            LunaLog.Log($"[LMP]: Setting otherNode on {vessel.vesselName}" +
                                $" part {dockedPartner.part?.partName}({dockedPartner.part?.flightID})" +
                                $" -> partner {node.part?.partName}({node.part?.flightID})");
                            dockedPartner.otherNode = node;
                        }
                        if (dockedPartner.part != null && node.dockedPartUId != dockedPartner.part.flightID)
                        {
                            LunaLog.Log($"[LMP]: Fixing dockedPartUId on {node.part?.partName}" +
                                $"({node.part?.flightID}): {node.dockedPartUId} -> {dockedPartner.part.flightID}");
                            node.dockedPartUId = dockedPartner.part.flightID;
                        }
                        if (node.part != null && dockedPartner.dockedPartUId != node.part.flightID)
                        {
                            LunaLog.Log($"[LMP]: Fixing dockedPartUId on {dockedPartner.part?.partName}" +
                                $"({dockedPartner.part?.flightID}): {dockedPartner.dockedPartUId} -> {node.part.flightID}");
                            dockedPartner.dockedPartUId = node.part.flightID;
                        }
                    }
                    else
                    {
                        LunaLog.Log($"[LMP]: Clearing ghost docked state on {vessel.vesselName}" +
                            $" part {node.part?.partName}" +
                            $" (flightID {node.part?.flightID})" +
                            $" fsm='{node.fsm.currentStateName}'" +
                            $" serialized='{node.state}'" +
                            $" dockedUId={node.dockedPartUId}");
                        node.otherNode = null;
                        node.dockedPartUId = 0;
                        node.state = "Ready";
                        node.fsm.StartFSM("Ready");
                    }

                    continue;
                }

                var fsmState = node.fsm.currentStateName;
                var serializedState = node.state;

                // Part tree is ground truth for physical connections.
                // Fall back to dockedPartUId with tree validation.
                var partner = FindPartnerFromPartTree(node);
                if (partner == null && node.dockedPartUId != 0)
                {
                    var byUid = FindPartnerByUId(node.dockedPartUId, vessel);
                    if (byUid != null && FindPartnerFromPartTree(byUid) == node)
                        partner = byUid;
                }

                // If we found a partner, recover both sides together
                if (partner != null)
                {
                    string nodeState, partnerState;
                    InferDockerDockeeRoles(node, partner, out nodeState, out partnerState);
                    RecoverDockedPair(node, partner, nodeState, partnerState,
                        $"serialized='{serializedState}' fsm='{fsmState}' dockedUId={node.dockedPartUId}",
                        vessel);
                    continue;
                }

                // Stale cross-vessel reference: dockedPartUId points to a part
                // on another vessel (left over from a previous undock).  Clean
                // it up so the port is fully available for re-docking.
                if (node.dockedPartUId != 0 && FindPartnerByUId(node.dockedPartUId) != null)
                {
                    LunaLog.Log($"[LMP]: Clearing stale cross-vessel dockedPartUId={node.dockedPartUId}" +
                        $" on {vessel.vesselName} part {node.part?.partName}" +
                        $" (flightID {node.part?.flightID})");
                    node.dockedPartUId = 0;
                    node.otherNode = null;
                    if (fsmState != "Ready")
                        node.fsm.StartFSM("Ready");
                    continue;
                }

                // Case 4: Stuck in transient state with no partner anywhere — reset to Ready
                if (IsInRecoverableTransientState(node))
                {
                    LunaLog.Log($"[LMP]: Docking port stuck in transient state '{fsmState}' " +
                        $"with no partner on {vessel.vesselName} part {node.part?.partName} " +
                        $"(flightID {node.part?.flightID}) — resetting to Ready");
                    node.fsm.StartFSM("Ready");
                    continue;
                }

                // Clean stale metadata: no valid partner found but port has
                // leftover dockUId/otherNode from a previous docking.
                if (node.dockedPartUId != 0 || node.otherNode != null)
                {
                    LunaLog.Log($"[LMP]: Cleaning stale metadata on {vessel.vesselName}" +
                        $" part {node.part?.partName} (flightID {node.part?.flightID})" +
                        $" fsm='{fsmState}' dockedUId={node.dockedPartUId}" +
                        $" otherNode={(node.otherNode != null ? node.otherNode.part?.flightID.ToString() : "null")}");
                    node.dockedPartUId = 0;
                    node.otherNode = null;
                }
            }
        }
    }
}

using System;
using System.Globalization;

namespace LmpClient.VesselUtilities
{
    /// <summary>
    /// Defensive rewriter for non-finite double tokens ("Infinity"/"+Infinity"/"-Infinity"/"NaN",
    /// any casing) in the <c>DISCOVERY</c> sub-node of a vessel <see cref="ConfigNode"/>.
    ///
    /// Why this exists at all: stock KSP serialises <see cref="DiscoveryInfo"/>'s
    /// <c>lifetime</c> / <c>refTime</c> / <c>lastObservedTime</c> via the default
    /// <see cref="double.ToString()"/>, which emits the literal string <c>"Infinity"</c> for
    /// <see cref="double.PositiveInfinity"/>. On load it parses them back via the single-arg
    /// <see cref="double.Parse(string)"/> inside <c>DiscoveryInfo.Load</c>, which throws
    /// <see cref="FormatException"/> on the <c>"Infinity"</c> token across the Mono runtime
    /// / cultures KSP ships on. Once the parse throws, <c>ProtoVessel.Load</c> unwinds
    /// half-instantiated and the vessel is dead on this client every time the server resends
    /// it.
    ///
    /// Why two ingress points (and why a shared helper):
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="VesselSerializer.CreateSafeProtoVesselFromConfigNode"/> sanitises the raw
    /// wire-side <see cref="ConfigNode"/> *before* <c>new ProtoVessel(...)</c>. This catches
    /// vessels that arrive from the server with <c>"Infinity"</c> already in the
    /// <c>DISCOVERY</c> node (asteroids/comets that round-tripped through the bug on a peer).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="VesselLoader"/> sanitises <c>protoVessel.discoveryInfo</c> *after*
    /// construction and immediately before <c>vesselProto.Load(...)</c>. This catches the
    /// case the wire-side pass cannot see: when the input <see cref="ConfigNode"/> has no
    /// <c>DISCOVERY</c> sub-node at all (typical for stations / probes / EVA / flag vessels
    /// that the originating client never had to give a tracking lifetime to), stock KSP's
    /// <c>ProtoVessel</c> constructor synthesises a default <c>DiscoveryInfo</c> with
    /// <see cref="double.PositiveInfinity"/> for both <c>lifetime</c> and <c>refTime</c> and
    /// serialises it back into <c>protoVessel.discoveryInfo</c> as the literal string
    /// <c>"Infinity"</c>. From the wire-side pass's perspective there was nothing to fix;
    /// the bad values only appear post-construction.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// We replace the offending tokens with a large finite sentinel
    /// (<see cref="FiniteSentinelSeconds"/>) formatted in invariant culture. This preserves
    /// the original semantic ("effectively never expires") while remaining a well-formed
    /// double that round-trips through the broken Mono parse path. NaN gets the same
    /// treatment because the parse on NaN is just as fragile and a NaN lifetime has no
    /// meaningful interpretation anyway.
    /// </summary>
    public static class DiscoveryInfoSanitizer
    {
        // Stand-in we write in place of "Infinity"/"-Infinity" / "NaN" tokens. 1e20 (in
        // seconds) is ~3.17e12 KSP-years -- effectively infinite for any game-time
        // lifetime/refTime comparison, while still being a well-formed finite double that
        // round-trips through Mono's broken single-arg Double.Parse on every culture/runtime
        // KSP ships on. We deliberately do NOT use double.MaxValue: its "R" round-trip
        // string ("1.7976931348623157E+308") has historically tripped the same Mono parse
        // path on a subset of installs, and we have no upside from sitting that close to
        // the type ceiling.
        public const double FiniteSentinelSeconds = 1e20;

        // Default `state` we write when DISCOVERY/state is missing. -1 (all bits set) is
        // the value stock KSP uses for "fully discovered / owned" vessels (the vast
        // majority of what flies in LMP -- stations, ships, probes, relays, EVAs, flags --
        // none of which the player needs to "discover" the way they discover an asteroid).
        // We preserve any pre-existing `state` value untouched; this default is only used
        // when the key is entirely absent, which is the case the synthesized-default code
        // path on the originating peer ends up producing.
        private const string DefaultStateValue = "-1";

        // Default `size` we write when DISCOVERY/size is missing. UntrackedObjectClass.A
        // (==0) is a safe Enum.Parse target on every KSP version we run against; the value
        // is meaningless for non-asteroid vessels (which is what we're defending here) and
        // is preserved untouched when the originating data already has it.
        private const string DefaultSizeValue = "0";

        /// <summary>
        /// Sanitises the <c>DISCOVERY</c> sub-node of <paramref name="vesselNode"/> in place.
        /// No-ops cleanly when the node is missing or has no offending values, so it is
        /// safe to call unconditionally on every wire-side vessel <see cref="ConfigNode"/>.
        /// </summary>
        /// <param name="vesselNode">Top-level vessel <see cref="ConfigNode"/> (the same node
        /// that gets handed to <c>new ProtoVessel(node, game)</c>).</param>
        /// <param name="vesselId">Used only for diagnostic log lines.</param>
        /// <param name="origin">Short tag distinguishing which ingress point invoked the
        /// sanitiser, so a single log line tells you whether the bad values arrived over
        /// the wire or were synthesised by stock KSP after construction.</param>
        /// <returns>Number of values rewritten (0 when nothing needed fixing).</returns>
        public static int SanitizeVesselNode(ConfigNode vesselNode, Guid vesselId, string origin)
        {
            return SanitizeDiscoveryNode(vesselNode?.GetNode("DISCOVERY"), vesselId, origin);
        }

        /// <summary>
        /// Sanitises a <c>DISCOVERY</c> <see cref="ConfigNode"/> directly. Used by the
        /// pre-Load pass in <see cref="VesselLoader"/> as part of
        /// <see cref="EnsureSafeDiscoveryInfo"/>, and also exposed for any future caller
        /// that already holds the inner node and just wants the sanitisation step.
        /// </summary>
        public static int SanitizeDiscoveryNode(ConfigNode discoveryNode, Guid vesselId, string origin)
        {
            if (discoveryNode == null) return 0;

            var rewrites = 0;
            rewrites += RewriteNonFiniteDouble(discoveryNode, "lifetime", vesselId, origin);
            rewrites += RewriteNonFiniteDouble(discoveryNode, "refTime", vesselId, origin);
            rewrites += RewriteNonFiniteDouble(discoveryNode, "lastObservedTime", vesselId, origin);

            if (rewrites > 0)
            {
                LunaLog.Log($"[LMP]: Sanitised {rewrites} non-finite DiscoveryInfo value(s) " +
                            $"on vessel {vesselId} ({origin}) to keep ProtoVessel.Load from " +
                            $"throwing FormatException in DiscoveryInfo.Load.");
            }
            return rewrites;
        }

        /// <summary>
        /// Last-line-of-defense pass for <see cref="ProtoVessel.discoveryInfo"/> that runs
        /// immediately before <c>vesselProto.Load(...)</c>. Guarantees the post-condition
        /// "<see cref="ProtoVessel.discoveryInfo"/> is a non-null <see cref="ConfigNode"/>
        /// whose <c>state</c> / <c>lastObservedTime</c> / <c>lifetime</c> / <c>refTime</c>
        /// / <c>size</c> values are all present and finite", which is the precondition that
        /// keeps stock KSP's <c>DiscoveryInfo.Load</c> off its synthesise-then-parse path.
        ///
        /// Why a presence-AND-finiteness pass and not just the finiteness pass that lived
        /// here originally: empirically (per LMP server traces), some vessels arrive with
        /// no <c>DISCOVERY</c> sub-node at all in their wire <see cref="ConfigNode"/>, and
        /// stock KSP's <c>ProtoVessel</c> constructor leaves <c>discoveryInfo</c> null /
        /// empty in that case. When <c>ProtoVessel.Load</c> later runs, it synthesises a
        /// default <see cref="DiscoveryInfo"/> with <see cref="double.PositiveInfinity"/>
        /// for <c>lifetime</c> and <c>refTime</c>, serialises them back into
        /// <c>this.discoveryInfo</c> via the default <see cref="double.ToString()"/> (i.e.
        /// the literal string "Infinity"), and then immediately re-parses them through
        /// <c>DiscoveryInfo.Load</c> -- the same Mono parse path that throws
        /// <see cref="FormatException"/> on the "Infinity" token. Because that whole
        /// synthesise-and-parse cycle happens *inside* <c>vesselProto.Load(...)</c>, the
        /// finiteness-only pass had no node to fix at the moment we ran it (the offending
        /// values didn't exist yet) and the original failure reproduced unchanged.
        ///
        /// By pre-populating a complete and finite <c>DISCOVERY</c> node here, we steer
        /// stock KSP's loader into its "use the provided node" branch and the synthesise
        /// path never runs. Any pre-existing values are preserved untouched (we only set
        /// keys that are missing); only non-finite values get rewritten via the normal
        /// sanitisation pass.
        /// </summary>
        /// <returns>True if any field was added or rewritten; false if the proto already
        /// satisfied the post-condition. Used purely so the caller can decide whether to
        /// emit a single summary log line.</returns>
        public static bool EnsureSafeDiscoveryInfo(ProtoVessel vesselProto)
        {
            if (vesselProto == null) return false;

            var changed = false;
            if (vesselProto.discoveryInfo == null)
            {
                vesselProto.discoveryInfo = new ConfigNode("DISCOVERY");
                changed = true;
            }

            var node = vesselProto.discoveryInfo;
            var vesselId = vesselProto.vesselID;
            var sentinel = FiniteSentinelSeconds.ToString("R", CultureInfo.InvariantCulture);

            //Three doubles use the strict "present AND non-empty AND parseable as finite
            //double" check. The empty-string case is observed in the wild (a peer wrote
            //out `lifetime = ` with no value) -- HasValue returns true for the empty key,
            //so a simple AddIfMissing pass would leave it untouched and `Double.Parse("")`
            //would throw FormatException inside DiscoveryInfo.Load. The non-finite case
            //("Infinity"/"NaN") has the same exception shape via a different code path.
            //EnsureFiniteDouble unifies both into one rewrite if the current value isn't
            //a value Double.Parse on Mono will accept.
            if (EnsureFiniteDouble(node, "lifetime", sentinel, vesselId)) changed = true;
            if (EnsureFiniteDouble(node, "refTime", sentinel, vesselId)) changed = true;
            if (EnsureFiniteDouble(node, "lastObservedTime", "0", vesselId)) changed = true;

            //state and size are int-typed in stock KSP; their parse path uses int.Parse
            //which doesn't share the Mono Double.Parse("Infinity") bug. We only need to
            //ensure the keys are present so KSP doesn't enter its synthesise-default
            //branch on this vessel.
            if (AddIfMissing(node, "state", DefaultStateValue)) changed = true;
            if (AddIfMissing(node, "size", DefaultSizeValue)) changed = true;

            if (changed)
            {
                LunaLog.Log($"[LMP]: Vessel {vesselId} pre-Load DiscoveryInfo normalised " +
                            $"(state={node.GetValue("state")}, " +
                            $"lifetime={node.GetValue("lifetime")}, " +
                            $"refTime={node.GetValue("refTime")}, " +
                            $"lastObservedTime={node.GetValue("lastObservedTime")}, " +
                            $"size={node.GetValue("size")}). " +
                            $"This blocks ProtoVessel.Load's synthesise-then-parse path that " +
                            $"throws FormatException on \"Infinity\"/\"\" inside DiscoveryInfo.Load.");
            }
            return changed;
        }

        /// <summary>
        /// Ensures <paramref name="node"/>[<paramref name="key"/>] holds a value that
        /// stock KSP's <c>Double.Parse(string)</c> on Mono will accept as a finite double.
        /// If the key is missing, the value is null/empty/whitespace, or the value is one
        /// of the non-finite tokens recognised by <see cref="IsNonFiniteDoubleToken"/>,
        /// rewrites it to <paramref name="replacement"/>. Returns true when a write
        /// occurred. Pre-existing finite values are preserved untouched.
        /// </summary>
        private static bool EnsureFiniteDouble(ConfigNode node, string key, string replacement, Guid vesselId)
        {
            var raw = node.GetValue(key);
            string reason;
            if (raw == null)
            {
                reason = "missing";
            }
            else if (string.IsNullOrWhiteSpace(raw))
            {
                reason = $"empty('{raw}')";
            }
            else if (IsNonFiniteDoubleToken(raw.Trim(), out _))
            {
                reason = $"non-finite('{raw}')";
            }
            else
            {
                //Treat as already-fine. We deliberately do NOT TryParse here: the whole
                //point of this helper is that Mono's Double.Parse on the values stock KSP
                //is about to read can throw on tokens TryParse would happily accept (in
                //particular "Infinity"). Filtering by the exact non-finite token set
                //above is what gives us a consistent reproducer-driven check.
                return false;
            }

            if (raw == null) node.AddValue(key, replacement);
            else node.SetValue(key, replacement);

            LunaLog.Log($"[LMP]: Vessel {vesselId} (pre-Load) DISCOVERY/{key} {reason}; " +
                        $"setting to '{replacement}' to survive DiscoveryInfo.Load.");
            return true;
        }

        /// <summary>
        /// Adds <paramref name="key"/>=<paramref name="value"/> to <paramref name="node"/>
        /// only if <paramref name="key"/> is not already present. Returns true when the
        /// add happened, false when the key was already there (server-provided int values
        /// for state/size go through int.Parse, which doesn't suffer the Mono "Infinity"
        /// bug, so leaving non-empty existing values alone is safe).
        /// </summary>
        private static bool AddIfMissing(ConfigNode node, string key, string value)
        {
            if (node.HasValue(key)) return false;
            node.AddValue(key, value);
            return true;
        }

        /// <summary>
        /// If <paramref name="node"/>[<paramref name="key"/>] holds a non-finite token,
        /// replace it with the finite sentinel (signed appropriately for "-Infinity") and
        /// return 1. Returns 0 for missing keys and for values that are already finite or
        /// otherwise non-matching (we deliberately leave malformed-but-finite-looking
        /// strings alone -- those are stock KSP's problem to surface, not ours to mutate).
        /// </summary>
        private static int RewriteNonFiniteDouble(ConfigNode node, string key, Guid vesselId, string origin)
        {
            var raw = node.GetValue(key);
            if (string.IsNullOrEmpty(raw)) return 0;

            var trimmed = raw.Trim();
            if (!IsNonFiniteDoubleToken(trimmed, out var negative)) return 0;

            var replacement = (negative ? -FiniteSentinelSeconds : FiniteSentinelSeconds)
                .ToString("R", CultureInfo.InvariantCulture);
            node.SetValue(key, replacement);
            LunaLog.Log($"[LMP]: Vessel {vesselId} ({origin}) DISCOVERY/{key}='{raw}' is " +
                        $"non-finite; rewriting to '{replacement}' to survive DiscoveryInfo.Load.");
            return 1;
        }

        /// <summary>
        /// Returns true for the case-insensitive token set { "Infinity", "+Infinity",
        /// "-Infinity", "NaN" } -- the exact strings stock <see cref="double.ToString()"/>
        /// emits for the corresponding non-finite values on the cultures KSP ships on
        /// (InvariantCulture and en-US both use "Infinity" / "NaN"). We deliberately do NOT
        /// try <see cref="double.TryParse(string,out double)"/> first: the whole reason
        /// this helper exists is that the runtime parse on these tokens is the bug we are
        /// working around, so relying on it here would defeat the purpose.
        /// </summary>
        private static bool IsNonFiniteDoubleToken(string value, out bool negative)
        {
            negative = false;
            if (string.Equals(value, "Infinity", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "+Infinity", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(value, "-Infinity", StringComparison.OrdinalIgnoreCase))
            {
                negative = true;
                return true;
            }
            //NaN has no meaningful sign for our purposes; just treat as positive sentinel.
            return string.Equals(value, "NaN", StringComparison.OrdinalIgnoreCase);
        }
    }
}

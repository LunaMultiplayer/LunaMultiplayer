using Server.Context;
using Server.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Server.System
{
    /// <summary>
    /// Tracks the cumulative amount of universal time each player has added to the server by creating new
    /// subspaces that advance the server's latest known time. Persisted as one line per player in
    /// <c>WarpAllotments.txt</c> inside the universe directory, alongside <c>Subspace.txt</c>, so server
    /// operators can see who has been pushing the server clock forward and by how much.
    ///
    /// <para>Line format (round-trippable):</para>
    /// <code>
    /// &lt;PlayerId&gt; (&lt;PlayerName&gt;) server warp allotment: &lt;y&gt; Years, &lt;d&gt; Days, &lt;h&gt; Hours
    /// </code>
    ///
    /// <para>
    /// The breakdown uses the stock KSP Kerbin calendar (1 day = 6 hours, 1 year = 426 days = 2556 hours)
    /// to match what an in-game player would see in the planetarium. KSP has no concept of months, so the
    /// breakdown deliberately omits a Months field. The numbers are stable across runs because we always
    /// re-derive the breakdown from a single integer hour total. Deltas below
    /// <see cref="MinimumTrackedSeconds"/> (1 hour) are ignored per the operator-facing requirement.
    /// </para>
    ///
    /// <para>
    /// The server runs headless and cannot reference the KSP assembly, so the calendar constants below are
    /// the stock Kerbin values hard-coded. Sub-hour fractional differences between the various exact KSP
    /// year lengths shipped over the years (~9_201_600 s vs ~9_203_545 s) are well below our 1-hour
    /// granularity floor and are deliberately ignored.
    /// </para>
    ///
    /// <para>
    /// Backwards compatibility: legacy lines using the previous Julian-calendar layout
    /// (<c>... Years, M Months, D Days, H Hours</c>) are still parsed and their hour total is preserved
    /// exactly; only the breakdown units change on the next rewrite. Existing operator allotments carry
    /// forward without manual migration. Anything that does not parse in either format is preserved
    /// verbatim so an operator's notes / comments are not silently discarded.
    /// </para>
    ///
    /// <para>
    /// Reads, mutates and writes go through <see cref="FileHandler"/> which serialises file access by path,
    /// and a class-local lock ensures the read-modify-write is atomic with respect to other warp events on
    /// the server.
    /// </para>
    /// </summary>
    public static class WarpAllotmentTracker
    {
        /// <summary>Warps shorter than this are not recorded, per the operator-facing requirement.</summary>
        public const double MinimumTrackedSeconds = 3600.0;

        // Stock KSP Kerbin calendar: 6-hour day, 426-day year. No "month" unit exists in KSP.
        private const long HoursPerKerbinDay = 6L;
        private const long DaysPerKerbinYear = 426L;
        private const long HoursPerKerbinYear = DaysPerKerbinYear * HoursPerKerbinDay; // 2556

        // Used only for parsing legacy entries that were written under the previous Julian breakdown.
        private const long HoursPerLegacyDay = 24L;
        private const long HoursPerLegacyMonth = 30L * HoursPerLegacyDay; // 720

        private static string AllotmentFile { get; } = Path.Combine(ServerContext.UniverseDirectory, "WarpAllotments.txt");

        /// <summary>
        /// Matches a well-formed allotment line in either the current Kerbin format
        /// (<c>... Years, D Days, H Hours</c>) or the legacy Julian format
        /// (<c>... Years, M Months, D Days, H Hours</c>). The Months segment is captured as an optional
        /// non-capturing group so a single regex covers both layouts without having to maintain two.
        ///
        /// <para>
        /// The player-id token is non-greedy and bounded by the literal " (" that introduces the player
        /// name, so player ids that contain whitespace are still accepted as a single field as long as
        /// they don't themselves contain " (". The player name is captured greedily up to the literal
        /// ") server warp allotment:" tail so names containing parentheses are tolerated.
        /// </para>
        /// </summary>
        private static readonly Regex LineRegex = new Regex(
            @"^(?<id>.+?) \((?<name>.*)\) server warp allotment:\s+(?<y>\d+) Years,\s*(?:(?<m>\d+) Months,\s*)?(?<d>\d+) Days,\s*(?<h>\d+) Hours\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly object MutationLock = new object();

        /// <summary>
        /// Add <paramref name="deltaSeconds"/> to <paramref name="playerId"/>'s running total. Only forward
        /// (positive) deltas of at least <see cref="MinimumTrackedSeconds"/> are recorded; everything else is
        /// a no-op. Safe to call from any thread.
        /// </summary>
        public static void RecordWarp(string playerId, string playerName, double deltaSeconds)
        {
            if (string.IsNullOrEmpty(playerId)) return;
            if (double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds)) return;
            if (deltaSeconds < MinimumTrackedSeconds) return;

            // Truncate to whole hours so the breakdown round-trips exactly through the file.
            var deltaHours = (long)(deltaSeconds / 3600.0);
            if (deltaHours <= 0) return;

            lock (MutationLock)
            {
                try
                {
                    var entries = LoadEntries(out var passthroughLines);
                    if (!entries.TryGetValue(playerId, out var entry))
                    {
                        entry = new Entry { PlayerId = playerId, PlayerName = playerName, TotalHours = 0 };
                        entries[playerId] = entry;
                    }
                    else if (!string.IsNullOrEmpty(playerName))
                    {
                        // Keep the latest known display name; the id is the stable key.
                        entry.PlayerName = playerName;
                    }

                    entry.TotalHours = SafeAdd(entry.TotalHours, deltaHours);

                    WriteEntries(entries, passthroughLines);
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Failed to update warp allotment for player '{playerId}' ({playerName}): {e}");
                }
            }
        }

        private sealed class Entry
        {
            public string PlayerId;
            public string PlayerName;
            public long TotalHours;
        }

        private static Dictionary<string, Entry> LoadEntries(out List<string> passthroughLines)
        {
            var entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
            passthroughLines = new List<string>();

            if (!FileHandler.FileExists(AllotmentFile)) return entries;

            foreach (var rawLine in FileHandler.ReadFileLines(AllotmentFile))
            {
                if (rawLine == null) continue;
                var line = rawLine.TrimEnd('\r');
                if (string.IsNullOrWhiteSpace(line)) continue;

                var match = LineRegex.Match(line);
                if (!match.Success)
                {
                    // Preserve unrecognised content (comments, hand edits) so we don't clobber operator notes.
                    passthroughLines.Add(line);
                    continue;
                }

                var id = match.Groups["id"].Value;
                var name = match.Groups["name"].Value;
                var totalHours = ParseTotalHours(match);

                // If a duplicate id ever appears (e.g. an operator manually merged files), fold the totals.
                if (entries.TryGetValue(id, out var existing))
                {
                    existing.TotalHours = SafeAdd(existing.TotalHours, totalHours);
                    if (string.IsNullOrEmpty(existing.PlayerName)) existing.PlayerName = name;
                }
                else
                {
                    entries[id] = new Entry { PlayerId = id, PlayerName = name, TotalHours = totalHours };
                }
            }

            return entries;
        }

        /// <summary>
        /// Reconstitute the running hour total from a parsed line. Legacy Julian-format entries (with a
        /// Months segment, 24-hour days) and current Kerbin-format entries (no Months segment, 6-hour days)
        /// are both accepted; the units of each segment are interpreted to match the format that produced
        /// the line so the underlying hour total is preserved exactly across the migration.
        /// </summary>
        private static long ParseTotalHours(Match match)
        {
            var years = long.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture);
            var days = long.Parse(match.Groups["d"].Value, CultureInfo.InvariantCulture);
            var hours = long.Parse(match.Groups["h"].Value, CultureInfo.InvariantCulture);

            var monthsGroup = match.Groups["m"];
            if (monthsGroup.Success)
            {
                var months = long.Parse(monthsGroup.Value, CultureInfo.InvariantCulture);
                return SafeAdd(
                    SafeAdd(
                        SafeMul(years, HoursPerLegacyDay * 365L), // legacy Julian year
                        SafeMul(months, HoursPerLegacyMonth)),
                    SafeAdd(
                        SafeMul(days, HoursPerLegacyDay),
                        hours));
            }

            return SafeAdd(
                SafeAdd(
                    SafeMul(years, HoursPerKerbinYear),
                    SafeMul(days, HoursPerKerbinDay)),
                hours);
        }

        private static void WriteEntries(Dictionary<string, Entry> entries, List<string> passthroughLines)
        {
            var sb = new StringBuilder();
            foreach (var line in passthroughLines)
            {
                sb.Append(line).Append(Environment.NewLine);
            }

            // Stable ordering: highest allotment first, then by id, so operators can see top contributors.
            Entry[] ordered;
            {
                var arr = new Entry[entries.Count];
                entries.Values.CopyTo(arr, 0);
                Array.Sort(arr, (a, b) =>
                {
                    var byTotal = b.TotalHours.CompareTo(a.TotalHours);
                    return byTotal != 0 ? byTotal : string.CompareOrdinal(a.PlayerId, b.PlayerId);
                });
                ordered = arr;
            }

            foreach (var entry in ordered)
            {
                sb.Append(FormatLine(entry.PlayerId, entry.PlayerName, entry.TotalHours)).Append(Environment.NewLine);
            }

            FileHandler.WriteToFile(AllotmentFile, sb.ToString());
        }

        private static string FormatLine(string playerId, string playerName, long totalHours)
        {
            if (totalHours < 0) totalHours = 0;

            var years = totalHours / HoursPerKerbinYear;
            var rem = totalHours - years * HoursPerKerbinYear;
            var days = rem / HoursPerKerbinDay;
            var hours = rem - days * HoursPerKerbinDay;

            return string.Format(CultureInfo.InvariantCulture,
                "{0} ({1}) server warp allotment: {2} Years, {3} Days, {4} Hours",
                playerId, playerName ?? string.Empty, years, days, hours);
        }

        private static long SafeAdd(long a, long b)
        {
            // Saturating add - keeps the file readable even if a griefer warps trillions of years.
            try { return checked(a + b); }
            catch (OverflowException) { return long.MaxValue; }
        }

        private static long SafeMul(long a, long b)
        {
            try { return checked(a * b); }
            catch (OverflowException) { return long.MaxValue; }
        }
    }
}

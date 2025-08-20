using System;

namespace Rankora_API.Scripts.Rankora.Types
{
    /// <summary>
    /// Represents metadata information about a leaderboard.
    /// </summary>
    [Serializable]
    public sealed class LeaderboardMetadata
    {
        /// <summary>
        /// Unique identifier of the leaderboard.
        /// </summary>
        public string id;

        /// <summary>
        /// The human-readable name of the leaderboard.
        /// </summary>
        public string name;

        /// <summary>
        /// The default sorting order for entries in the leaderboard.
        /// Possible values: "asc" (ascending) or "desc" (descending).
        /// </summary>
        public string sort_order;

        /// <summary>
        /// The default score format used by the leaderboard.
        /// Possible values: "number" (numeric scores) or "time" (time-based scores).
        /// </summary>
        public string score_format;

        /// <summary>
        /// Total number of entries currently on the leaderboard.
        /// </summary>
        public int total_entries;
    }
}

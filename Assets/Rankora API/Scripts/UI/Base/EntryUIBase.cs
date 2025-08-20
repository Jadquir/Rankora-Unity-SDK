using Rankora_API.Scripts.Rankora.Types;
using UnityEngine;

namespace Rankora_API.Scripts.UI.Base
{
    public abstract class EntryUIBase : MonoBehaviour
    {
        /// <summary>
        /// Sets UI elements to display data from the given leaderboard entry.
        /// Applies special formatting and styles for top entries.
        /// </summary>
        /// <param name="entry">Leaderboard entry data to display</param>
        public abstract void SetEntry(LeaderboardEntry entry);
    }
}

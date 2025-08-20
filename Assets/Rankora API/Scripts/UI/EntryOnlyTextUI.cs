using Rankora_API.Scripts.Rankora.Types;
using Rankora_API.Scripts.UI.Base;
using Rankora_API.Scripts.Utils;
using System;
using TMPro;
using UnityEngine;

namespace Rankora_API.Scripts.UI
{
    /// <summary>
    /// Manages the UI display for a single leaderboard entry.
    /// Shows rank, player name, score, and applies styling for top ranks.
    /// </summary>
    [AddComponentMenu("Rankora/UI/Leaderboard Entry UI (Only Text)")]
    [Icon("Assets/Rankora API/Sprites/ScriptIcons/File.png")]
    internal class EntryOnlyTextUI : EntryUIBase
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text Text;          // Text component to show player

        /// <summary>
        /// Sets UI elements to display data from the given leaderboard entry.
        /// Applies special formatting and styles for top entries.
        /// </summary>
        /// <param name="entry">Leaderboard entry data to display</param>
        public override void SetEntry(LeaderboardEntry entry)
        {
            if (entry == null)
            {
                Debug.LogError("[Rankora API] Entry is null, cannot set entry UI.");
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            try
            {
                var rankoraSettings = RankoraSettings.Instance;


                var playerName = entry.is_current_player.GetValueOrDefault()
                    ? string.Format(rankoraSettings.CurrentPlayerTextFormat ?? "{0} (You)", entry.name)
                    : entry.name;

                var rankText = entry.rank.ToString();

                var playerScore = FormatScore.Format(entry.score, rankoraSettings.ScoreFormat);

                Text.text = $"{rankText} - {playerName} - {playerScore}";
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}

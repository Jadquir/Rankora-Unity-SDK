using Rankora_API.Scripts.Rankora.Api;
using Rankora_API.Scripts.Rankora.Types;
using Rankora_API.Scripts.UI.Base;
using Rankora_API.Scripts.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rankora_API.Scripts.UI
{
    /// <summary>
    /// Manages the UI display for a single leaderboard entry.
    /// Shows rank, player name, score, and applies styling for top ranks.
    /// </summary>
    [AddComponentMenu("Rankora/UI/Leaderboard Entry UI")]
    [Icon("Assets/Rankora API/Sprites/ScriptIcons/File.png")]
    public class EntryUI : EntryUIBase
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text Rank_Text;          // Text component to show player's rank
        [SerializeField] private TMP_Text PlayerName_Text;    // Text component to show player's name
        [SerializeField] private TMP_Text PlayerScore_Text;   // Text component to show player's score

        [Header("Background Elements")]
        [SerializeField] private Image RankBgImage;           // Background image for rank text
        [SerializeField] private Image BackgroundImage;       // Background image for the whole entry
        [SerializeField] private Outline BackgroundOutline;   // Outline effect on the background

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

                // Format player name; add "(You)" suffix if this is the current player
                PlayerName_Text.text = entry.is_current_player.GetValueOrDefault()
                    ? string.Format(rankoraSettings.CurrentPlayerTextFormat ?? "{0} (You)", entry.name)
                    : entry.name;

                // Set rank and score text
                Rank_Text.text = entry.rank.ToString();
                PlayerScore_Text.text = FormatScore.Format(entry.score, rankoraSettings.ScoreFormat);

                // Apply custom styles for top entries based on rank
                var styleList = rankoraSettings.EntryStyleAssets;
                if (styleList != null && styleList.Count > entry.rank - 1 && entry.rank > 0)
                {
                    var style = styleList[entry.rank - 1];
                    if (style != null)
                    {
                        if (style.backgroundColor != null)
                            BackgroundImage.color = style.backgroundColor;

                        if (style.outlineColor != null)
                            BackgroundOutline.effectColor = style.outlineColor;

                        if (style.outlineDistance != null)
                            BackgroundOutline.effectDistance = style.outlineDistance;

                        if (style.textColor != null)
                        {
                            PlayerName_Text.color = style.textColor;
                            PlayerScore_Text.color = style.textColor;
                        }

                        Rank_Text.color = style.rankTextColor;
                        RankBgImage.color = style.rankBackgroundColor;

                        if (Rank_Text.TryGetComponent<Outline>(out var rankOutline))
                        {
                            rankOutline.effectColor = style.rankOutlineColor;
                            rankOutline.effectDistance = style.rankOutlineDistance;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}

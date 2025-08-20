using Rankora_API.Scripts.Rankora.Main;
using Rankora_API.Scripts.Rankora.Types;
using Rankora_API.Scripts.Utils;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace Rankora_API.Scripts.UI
{
    /// <summary>
    /// Displays detailed information about the current player in a UI text element.
    /// Subscribes to player update events to refresh displayed info automatically.
    /// </summary>
    [AddComponentMenu("Rankora/UI/Player Info UI")]
    [Icon("Assets/Rankora API/Sprites/ScriptIcons/Info.png")]
    public class PlayerInfoUI : MonoBehaviour
    {
        public TMP_Text PlayerInfoText; // UI Text element to show player info

        private void Awake()
        {
            // Subscribe to player updates so UI refreshes when player data changes
            RankoraPlayer.Instance.OnPlayerUpdate.Subscribe(OnPlayerUpdated);

            // Trigger initial player data fetch
            RankoraPlayer.Instance.Get();
        }

        /// <summary>
        /// Called whenever player data is updated.
        /// Formats player details into a readable string and displays in the UI.
        /// </summary>
        private void OnPlayerUpdated(RankoraPlayer player)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Player ID: {player.PlayerId}");
            builder.AppendLine($"Name: {player.PlayerName}");
            builder.AppendLine($"Rank: {player.Rank}");
            builder.AppendLine($"Score: {FormatScore.Format(player.Score)} ({player.Score})");
            builder.AppendLine($"Created At: {player.CreatedAt}");
            builder.AppendLine($"Updated At: {player.UpdatedAt}");

            // Display metadata key-value pairs if available
            var playerMetadata = player.Metadata != null ? player.Metadata.GetData() : Enumerable.Empty<MetadataItem>();
            if (playerMetadata.Any())
            {
                builder.AppendLine("Metadata:");
                foreach (var kvp in playerMetadata)
                {
                    builder.AppendLine($"  {kvp.Key}: {kvp.Value}");
                }
            }
            else
            {
                builder.AppendLine("No metadata available.");
            }

            PlayerInfoText.text = builder.ToString();
        }

        /// <summary>
        /// Public method to manually refresh player data from the server.
        /// </summary>
        public void RefreshPlayerInfo()
        {
            RankoraPlayer.Instance.RefreshPlayerData();
        }
    }
}

using Assets.Rankora_API.Scripts.Visual.Scripts;
using Rankora_API.Examples.Visual.Scripts;
using Rankora_API.Scripts.Rankora.Api;
using Rankora_API.Scripts.Rankora.Main;
using Rankora_API.Scripts.Rankora.Types;
using Rankora_API.Scripts.UI.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rankora_API.Scripts.UI
{
    /// <summary>
    /// UI controller that displays the current player's leaderboard entry.
    /// Listens for updates from RankoraPlayer and automatically updates UI.
    /// Also handles hover effects and "See More Details" overlay.
    /// </summary>
    [RequireComponent(typeof(EntryUI))]
    [AddComponentMenu("Rankora/UI/Current Player Entry UI")]
    [Icon("Assets/Rankora API/Sprites/ScriptIcons/User.png")]
    public class CurrentPlayerEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // Cached reference to EntryUI component for setting display values
        private EntryUIBase _entryUI;
        public EntryUIBase EntryUI => _entryUI == null ? _entryUI = GetComponent<EntryUIBase>() : _entryUI;

        [SerializeField] Button SeeMoreDetailsButton; // Button to open player details overlay
        [SerializeField] Overlay PlayerInformationOverlay; // Overlay UI for showing details
        FadeObject fader; // Handles fade in/out for SeeMoreDetailsButton

        private void Awake()
        {
            // Create fade effect for the button with a short fade time
            fader = FadeObject.CreateFade(SeeMoreDetailsButton.transform, .1f);
            fader.DisableAfterFadeout = false; // Keep object active after fade out

            // Subscribe to player update events from Rankora
            RankoraPlayer.Instance.OnPlayerUpdate.Subscribe(OnPlayerUpdated);

            // Hide UI until we have player data
            gameObject.SetActive(false);

            // Request the player's data immediately
            RankoraPlayer.Instance.Get();

            // Set up the "See More Details" button click handler
            if (PlayerInformationOverlay != null)
            {
                SeeMoreDetailsButton.onClick.AddListener(() => { PlayerInformationOverlay.Open(); });
            }
        }

        /// <summary>
        /// Callback when the RankoraPlayer singleton notifies that player data has updated.
        /// </summary>
        private void OnPlayerUpdated(RankoraPlayer player)
        {
            OnPlayerFetched(player.PlayerEntry);
        }

        PlayerEntry _current; // Cached reference to the current player's entry data

        /// <summary>
        /// Handles incoming player entry data and updates the UI accordingly.
        /// </summary>
        private void OnPlayerFetched(PlayerEntry entry)
        {
            if (entry == null || entry.name == RankoraPlayer.UnkownUserText)
            {
                // Disable UI if player data is not available
                gameObject.SetActive(false);
                _current = null;
                return;
            }

            entry.is_current_player = true; // Explicitly mark this entry as the current player
            _current = entry;

            // Update the EntryUI to display the player's data
            EntryUI.SetEntry(_current);

            // Enable UI now that we have valid data
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Mouse hover enter event — fades in "See More Details" button.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (PlayerInformationOverlay == null) return;
            fader.FadeIn();
        }

        /// <summary>
        /// Mouse hover exit event — fades out "See More Details" button.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (PlayerInformationOverlay == null) return;
            fader.FadeOut();
        }
    }
}

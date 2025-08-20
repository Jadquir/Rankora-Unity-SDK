using Rankora_API.Scripts.Rankora.Api;
using Rankora_API.Scripts.Rankora.Main;
using Rankora_API.Scripts.Rankora.Types;
using Rankora_API.Scripts.UI.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Rankora_API.Scripts.UI
{
    /// <summary>
    /// Controls the leaderboard UI:
    /// - Displays leaderboard entries
    /// - Handles loading and error messages
    /// - Supports pagination: next, previous, and jump to page
    /// </summary>
    [AddComponentMenu("Rankora/UI/Leaderboard UI")]
    [Icon("Assets/Rankora API/Sprites/ScriptIcons/Trophy.png")]
    public class LeaderboardUI : LeaderboardUIBase
    {
        [Header("Prefabs")]
        public EntryUIBase entryPrefab;      // Prefab for individual leaderboard entries

        [Header("Leaderboard UI Elements")]
        [SerializeField] private Transform entriesPanel;       // Parent transform for entry UI elements
        [SerializeField] private TMP_Text ErrorText; // Text shown inside leaderboard for messages/errors
        [SerializeField] private Transform loadingIndicatorParent;   // Loading spinner or similar indicator
        [SerializeField] private TMP_Text loadingText;         // Text shown while loading
        [SerializeField] private TMP_Text currentPageText;         // Page number indicator
        [SerializeField] private Button previousPageButton;         // Previous Page Button
        [SerializeField] private Button nextPageButton;         // Next Page Button
        [SerializeField] private Button refreshPageButton;         // Refresh Button

        protected override void Awake()
        {
            if (!ValidateFields())
            {
                Debug.LogWarning($"{gameObject.name} is disabling due to missing references.");
                gameObject.SetActive(false);
                return;
            }
            if (previousPageButton != null)
            {
                previousPageButton.onClick.RemoveAllListeners();
                previousPageButton.onClick.AddListener(() => GetPreviousPage());
            }
            if (nextPageButton != null)
            {
                nextPageButton.onClick.RemoveAllListeners();
                nextPageButton.onClick.AddListener(() => GetNextPage());
            }
            if (refreshPageButton != null)
            {
                refreshPageButton.onClick.AddListener(() => RefreshEntries());
            }

            base.Awake();            
        }
        void UpdateButtons()
        {
            if (previousPageButton != null)
            {
                previousPageButton.interactable = CanGoPreviousPage;
            }
            if (nextPageButton != null)
            {
                nextPageButton.interactable = CanGoNextPage;
            }
        }
        private bool ValidateFields()
        {
            bool allValid = true;

            allValid &= CheckField(entriesPanel, "Entries Panel");
            allValid &= CheckField(ErrorText, "Error Text");
            allValid &= CheckField(loadingIndicatorParent, "Loading Indicator Parent");

            return allValid;
        }

        private bool CheckField(Object obj, string fieldName)
        {
            if (obj == null)
            {
                Debug.LogError($"The field '{fieldName}' is not assigned in {gameObject.name}.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Called when leaderboard entries should be displayed. Updates UI with the entries.
        /// </summary>
        /// <param name="entries">List of leaderboard entries to display</param>
        protected override void OnSetEntries(List<LeaderboardEntry> entries)
        {

            ClearEntries();

            foreach (var entry in entries)
            {
                AddEntry(entry);
            }

            UpdateButtons();
            
            if(currentPageText != null)
            {
                // Update the page indicator
                currentPageText.text = string.Format(
                    RankoraSettings.Instance.CurrentPageFormat,
                    RankoraLeaderboard.Instance.CurrentPageIndex + 1
                    );
            }
        }

        /// <summary>
        /// Called when a message should be displayed (error or info). Updates UI with the message.
        /// </summary>
        /// <param name="message">Message to display</param>
        protected override void OnSetMessage(string message)
        {
            ClearEntries();

            ErrorText.text = message;

            bool isTextActive = !string.IsNullOrEmpty(message);
            ErrorText.gameObject.SetActive(isTextActive);
            entriesPanel.gameObject.SetActive(!isTextActive);
        }

        /// <summary>
        /// Called when loading state changes. Shows/hides loading indicators.
        /// </summary>
        /// <param name="loading">True if loading, false otherwise</param>
        protected override void OnSetLoading(bool loading)
        {
            if(loadingText !=  null)
                loadingText.text = GetLoadingText();

            loadingIndicatorParent.gameObject.SetActive(loading);

            // Hide messages and entries panel while loading
            ErrorText.gameObject.SetActive(!loading);
            entriesPanel.gameObject.SetActive(!loading);

            UpdateButtons();
        }

        /// <summary>
        /// Clears all instantiated entry UI elements from the entries panel.
        /// </summary>
        private void ClearEntries()
        {
            if (entriesPanel != null)
            {
                foreach (Transform child in entriesPanel)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Adds a single leaderboard entry UI element to the entries panel.
        /// </summary>
        /// <param name="entry">Leaderboard entry data</param>
        private void AddEntry(LeaderboardEntry entry)
        {
            if (entriesPanel != null && entryPrefab != null)
            {
                var entryUI = Instantiate(entryPrefab, entriesPanel);
                entryUI.SetEntry(entry);
            }
        }
    }
}

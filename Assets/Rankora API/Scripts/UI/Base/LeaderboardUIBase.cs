using Rankora_API.Scripts.Rankora.Main;
using Rankora_API.Scripts.Rankora.Types;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rankora_API.Scripts.UI.Base
{
    /// <summary>
    /// Base class for leaderboard UI components that handles:
    /// - Event subscriptions to RankoraLeaderboard
    /// - Data management and caching
    /// - Abstract methods for UI updates that concrete implementations must override
    /// </summary>
    public abstract class LeaderboardUIBase : MonoBehaviour
    {
        [Header("Settings")]
        public int NumberOfEntries = 5;  // How many entries to show per page

        protected EntriesQuery query;      // Query for fetching leaderboard entries (optional caching)
        protected List<LeaderboardEntry> currentEntries; // Cached current entries
        protected bool isLoading; // Loading state
        protected string currentMessage; // Current message to display

        protected virtual void Awake()
        {
            // Subscribe to leaderboard events for updates and errors
            RankoraLeaderboard.Instance.OnEntriesUpdated.Subscribe(OnEntriesUpdated);
            RankoraLeaderboard.Instance.OnError.Subscribe(OnError);

            // Initialize leaderboard query and fetch first page
            SetLoading(true);
            RankoraLeaderboard.Instance.SetSettings(NumberOfEntries).SetPageFirstPage().GetCurrentPage();
        }

        protected virtual void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (RankoraLeaderboard.Instance != null)
            {
                RankoraLeaderboard.Instance.OnEntriesUpdated.Unsubscribe(OnEntriesUpdated);
                RankoraLeaderboard.Instance.OnError.Unsubscribe(OnError);
            }
        }
        /// <summary>
        /// <see cref="RankoraLeaderboard.CurrentPageIndex"/>
        /// </summary>
        public int CurrentPageIndex => RankoraLeaderboard.Instance.CurrentPageIndex;
#nullable enable
        /// <summary>
        /// Called when leaderboard entries are updated. Handles data caching and calls UI update methods.
        /// </summary>
        /// <param name="entries">Enumerable of leaderboard entries</param>
        protected virtual void OnEntriesUpdated(IEnumerable<LeaderboardEntry>? entries)
        {
            Debug.Log("Entries Updated on " + gameObject.name);
            SetLoading(false);
            currentMessage = "";

            if (entries == null || !entries.Any())
            {
                currentEntries = new List<LeaderboardEntry>();
                currentMessage = RankoraSettings.Instance.LeaderboardEmptyText;
                OnSetMessage(currentMessage);
                return;
            }

            OnSetMessage(string.Empty);


            currentEntries = entries.ToList();
            OnSetEntries(currentEntries);
        }

#nullable disable

        /// <summary>
        /// Called when an error occurs. Handles error message caching and calls UI update methods.
        /// </summary>
        /// <param name="message">Error message to display</param>
        protected virtual void OnError(string message)
        {
            SetLoading(false);
            currentMessage = message;
            currentEntries = new List<LeaderboardEntry>();
            OnSetMessage(message);
        }

        /// <summary>
        /// Sets loading state and calls UI update method.
        /// </summary>
        /// <param name="loading">True to show loading indicator, false otherwise</param>
        protected virtual void SetLoading(bool loading)
        {
            isLoading = loading;
            OnSetLoading(loading);
        }

        public bool CanGoPreviousPage => !isLoading && RankoraLeaderboard.Instance.CanGoPreviousPage;
        public bool CanGoNextPage => !isLoading && RankoraLeaderboard.Instance.CanGoNextPage;
        /// <summary>
        /// Refreshes the current leaderboard page by fetching data.
        /// Shows loading indicator during the fetch.
        /// </summary>
        public virtual void RefreshEntries()
        {
            SetLoading(true);
            RankoraLeaderboard.Instance.RefreshEntries();
        }

        /// <summary>
        /// Request the next page of leaderboard entries, if available.
        /// </summary>
        public virtual void GetNextPage()
        {
            if (!CanGoNextPage)
                return;
            SetLoading(true);
            RankoraLeaderboard.Instance.NextPage().GetCurrentPage();
        }

        /// <summary>
        /// Request the previous page of leaderboard entries, if available.
        /// </summary>
        public virtual void GetPreviousPage()
        {
            if (!CanGoPreviousPage)
                return;
            SetLoading(true);
            RankoraLeaderboard.Instance.PreviousPage().GetCurrentPage();
        }

        /// <summary>
        /// Request to jump to a specific page number (0-based).
        /// </summary>
        /// <param name="pageNumber">Zero-based page index</param>
        public virtual void GetPage(int pageNumber)
        {
            SetLoading(true);
            RankoraLeaderboard.Instance.SetPage(pageNumber).GetCurrentPage();
        }

        // Abstract methods that concrete implementations must override

        /// <summary>
        /// Called when leaderboard entries should be displayed. Override to update UI with the entries.
        /// </summary>
        /// <param name="entries">List of leaderboard entries to display</param>
        protected abstract void OnSetEntries(List<LeaderboardEntry> entries);

        /// <summary>
        /// Called when a message should be displayed (error or info). Override to update UI with the message.
        /// </summary>
        /// <param name="message">Message to display</param>
        protected abstract void OnSetMessage(string message);

        /// <summary>
        /// Called when loading state changes. Override to show/hide loading indicators.
        /// </summary>
        /// <param name="loading">True if loading, false otherwise</param>
        protected abstract void OnSetLoading(bool loading);

        // Helper methods that concrete implementations can use

        /// <summary>
        /// Gets the current loading text from settings.
        /// </summary>
        /// <returns>Loading text string</returns>
        protected virtual string GetLoadingText()
        {
            return RankoraSettings.Instance.LoadingLeaderboardText;
        }

        /// <summary>
        /// Gets the current entries for use in concrete implementations.
        /// </summary>
        /// <returns>Current leaderboard entries</returns>
        protected virtual List<LeaderboardEntry> GetCurrentEntries()
        {
            return currentEntries ?? new List<LeaderboardEntry>();
        }

        /// <summary>
        /// Gets the current loading state for use in concrete implementations.
        /// </summary>
        /// <returns>True if currently loading, false otherwise</returns>
        protected virtual bool GetIsLoading()
        {
            return isLoading;
        }

        /// <summary>
        /// Gets the current message for use in concrete implementations.
        /// </summary>
        /// <returns>Current message string</returns>
        protected virtual string GetCurrentMessage()
        {
            return currentMessage ?? "";
        }
    }
}

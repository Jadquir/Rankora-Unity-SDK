#nullable enable
using Rankora_API.Scripts.Rankora.Api;
using Rankora_API.Scripts.Rankora.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rankora_API.Scripts.Rankora.Main
{
    /// <summary>
    /// Handles leaderboard pagination and fetching.
    /// </summary>
    public class RankoraLeaderboard
    {
        /// <summary>
        /// Represents a single page of leaderboard entries.
        /// </summary>
        public class LeaderboardPage
        {
            private readonly EntriesQuery query;
            private int totalEntries;
            private List<LeaderboardEntry> entries = new();

            public string ErrorMessage { get; private set; } = string.Empty;

            /// <summary> Zero-based index of the current page. </summary>
            public int PageNumber => query.Skip / query.PageSize;

            /// <summary> Total number of available pages. </summary>
            public int TotalPages => (int)Math.Ceiling((double)totalEntries / query.PageSize);

            public LeaderboardPage(EntriesQuery query, int totalEntries)
            {
                this.query = query;
                this.totalEntries = totalEntries;
            }

            /// <summary> Returns the next page, or null if this is the last one. </summary>
            public LeaderboardPage? GetNextPage()
            {
                int nextSkip = query.Skip + query.PageSize;
                if (nextSkip >= totalEntries)
                    return null;

                return new LeaderboardPage(query.GetNextPageQuery(), totalEntries);
            }

            /// <summary> Returns the previous page, or null if this is the first one. </summary>
            public LeaderboardPage? GetPreviousPage()
            {
                if (query.Skip <= 0)
                    return null;

                return new LeaderboardPage(query.GetPreviousPageQuery(), totalEntries);
            }

            /// <summary> Returns the first page. </summary>
            public LeaderboardPage GetFirstPage()
            {
                var firstQuery = new EntriesQuery(query) { Skip = 0 };
                return new LeaderboardPage(firstQuery, totalEntries);
            }

            /// <summary>
            /// Sets the page to the given index.
            /// </summary>
            /// <param name="pageNumber">Zero-based page index.</param>
            public LeaderboardPage? SetPage(int pageNumber)
            {
                if (pageNumber < 0 || pageNumber >= TotalPages)
                    return null;

                var newQuery = new EntriesQuery(query) { Skip = pageNumber * query.PageSize };
                return new LeaderboardPage(newQuery, totalEntries);
            }

            /// <summary>
            /// Called internally when fetching is complete to store results.
            /// </summary>
            private void OnFetchComplete(LeaderboardEntriesResponse response)
            {
                if (!response.success || response.entries == null || response.entries.Length == 0)
                {
                    entries = new List<LeaderboardEntry>();
                    return;
                }

                totalEntries = response.pagination?.total ?? 0;
                entries = response.entries.ToList();
            }

            /// <summary>
            /// Fetches entries for this page. Uses cached entries if already fetched.
            /// </summary>
            internal void Get(Action<List<LeaderboardEntry>>? onFetchComplete = null, Action<string>? onError = null)
            {
                if (entries.Count > 0)
                {
                    onFetchComplete?.Invoke(entries);
                    return;
                }
                RefreshEntries(onFetchComplete, onError);
            }

            internal void RefreshEntries(Action<List<LeaderboardEntry>>? onFetchComplete = null, Action<string>? onError = null)
            {
                RankoraClient.GetLeaderboardEntries(query, (response) =>
                {
                    OnFetchComplete(response);
                    onFetchComplete?.Invoke(entries);
                },
                (error) =>
                {
                    ErrorMessage = error;
                    onError?.Invoke(ErrorMessage);
                });
            }
        }

        public static RankoraLeaderboard Instance { get; private set; } = new RankoraLeaderboard();

        public RankoraEvents.RankoraEvent<List<LeaderboardEntry>> OnEntriesUpdated { get; } =
            new();

        public RankoraEvents.RankoraEvent<string> OnError { get; } =
            new();

        private LeaderboardPage currentPage;

        /// <summary> Zero-based index of the current page. </summary>
        public int CurrentPageIndex => currentPage.PageNumber;
        private RankoraLeaderboard()
        {
            var query = new EntriesQuery
            {
                PageSize = RankoraSettings.Instance.PageSize,
                Order = RankoraSettings.Instance.LeaderboardSorting,
                Skip = 0
            };

            currentPage = new LeaderboardPage(query, 0);

            // Subscribe to player rank updates
            RankoraEvents.OnPlayerRankUpdated.Subscribe((newRank) =>
            {
                // Page boundaries are inclusive: (PageNumber * PageSize) + 1 to end
                int pageStartRank = currentPage.PageNumber * query.PageSize + 1;
                int pageEndRank = pageStartRank + query.PageSize - 1;

                // If rank falls inside or just moved out of this range, refresh
                if (newRank >= pageStartRank && newRank <= pageEndRank)
                {
                    // Player entered current page → refresh
                    Debug.Log($"RankoraLeaderboard: Player entered page {currentPage.PageNumber}, refreshing...");
                    GetCurrentPage();
                }
                else
                {
                    // Player may have been on page but moved out
                    // This is a quick force refresh to keep data in sync
                    Debug.Log($"RankoraLeaderboard: Player moved outside page {currentPage.PageNumber}, refreshing...");
                    GetCurrentPage();
                }
            });
        }
        /// <summary> Sets the page query with given settings. </summary>
        private void SetCurrentPageSettings(int numberOfEntries, Order sorting = Order.None)
        {
            var query = new EntriesQuery
            {
                PageSize = numberOfEntries,
                Order = sorting,
                Skip = 0
            };

            currentPage = new LeaderboardPage(query, 0);
        }

        /// <summary> Sets custom settings for fetching pages. </summary>
        public RankoraLeaderboard SetSettings(int numberOfEntries, Order sorting = Order.None)
        {
            SetCurrentPageSettings(numberOfEntries, sorting);
            return this;
        }

        /// <summary> Jumps to the first page. </summary>
        public RankoraLeaderboard SetPageFirstPage() => SetPage(0);

        /// <summary> Jumps to the given page number (0-based). </summary>
        public RankoraLeaderboard SetPage(int pageNumber)
        {
            var newPage = currentPage.SetPage(pageNumber);
            if (newPage != null) currentPage = newPage;
            return this;
        }

        public bool CanGoNextPage => currentPage.GetNextPage() != null;
        public bool CanGoPreviousPage => currentPage.GetPreviousPage() != null;
        /// <summary> Goes to the next page, if available. </summary>
        public RankoraLeaderboard NextPage()
        {
            var newPage = currentPage.GetNextPage();
            if (newPage != null) currentPage = newPage;
            return this;
        }

        /// <summary> Goes to the previous page, if available. </summary>
        public RankoraLeaderboard PreviousPage()
        {
            var newPage = currentPage.GetPreviousPage();
            if (newPage != null) currentPage = newPage;
            return this;
        }

        /// <summary>
        /// Fetches the current page's entries.
        /// </summary>
        public void GetCurrentPage(Action<List<LeaderboardEntry>>? onFetchComplete = null, Action<string>? onError = null)
        {
            currentPage.Get(WrapSuccess(onFetchComplete), WrapError(onError));
        }

        private Action<List<LeaderboardEntry>> WrapSuccess(Action<List<LeaderboardEntry>>? onFetchComplete)
        {
            return (response) =>
            {
                onFetchComplete?.Invoke(response);
                OnEntriesUpdated.Raise(response);
            };
        }

        private Action<string> WrapError(Action<string>? onError)
        {
            return (error) =>
            {
                onError?.Invoke(error);
                OnError.Raise(error);
            };
        }

        public void RefreshEntries(Action<List<LeaderboardEntry>>? onFetchComplete = null, Action<string>? onError = null)
        {
            currentPage.RefreshEntries(onFetchComplete, onError);
        }
    }
}

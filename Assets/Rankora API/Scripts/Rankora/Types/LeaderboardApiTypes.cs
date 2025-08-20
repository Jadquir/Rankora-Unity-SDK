//using EnhancedJSON;
using System;
using System.Collections.Generic;
using System.IO;

#nullable enable
namespace Rankora_API.Scripts.Rankora.Types
{
    /// <summary>
    /// Represents a single entry in a leaderboard.
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        /// <summary>
        /// Player's display name.
        /// </summary>
        public string name = string.Empty;

        /// <summary>
        /// Player's score for the leaderboard entry.
        /// </summary>
        public double score = -1;

        /// <summary>
        /// Player's rank position in the leaderboard.
        /// </summary>
        public int rank = -1;

        /// <summary>
        /// Optional metadata associated with the player or entry.
        /// </summary>
        public Metadata metadata = new();

        /// <summary>
        /// The date and time this entry was last updated.
        /// </summary>
        public DateTime updated_at;

        /// <summary>
        /// Indicates if this entry belongs to the current player.
        /// True if yes, false or null otherwise.
        /// </summary>
        public bool? is_current_player = null;
    }

    /// <summary>
    /// Extends <see cref="LeaderboardEntry"/> with player and leaderboard IDs and creation timestamp.
    /// </summary>
    [Serializable]
    public class PlayerEntry : LeaderboardEntry
    {
        /// <summary>
        /// Unique identifier for the player.
        /// </summary>
        public string player_id = string.Empty;

        /// <summary>
        /// Identifier of the leaderboard this entry belongs to.
        /// </summary>
        public string leaderboard_id = string.Empty;

        /// <summary>
        /// The date and time this entry was created.
        /// </summary>
        public DateTime created_at;
    }

    /// <summary>
    /// Data used to submit or update a player entry.
    /// </summary>
    [Serializable]
    public class PostPlayerData
    {
        /// <summary>
        /// Player's unique identifier. Leave empty to create new entry.
        /// </summary>
        public string player_id = string.Empty;

        /// <summary>
        /// Player's display name.
        /// </summary>
        public string name = string.Empty;

        /// <summary>
        /// Player's score to submit.
        /// </summary>
        public double score = -1;

        /// <summary>
        /// Optional metadata to submit along with score.
        /// </summary>
        public Metadata metadata = new();
    }

    /// <summary>
    /// Response received after submitting or updating a player entry.
    /// </summary>
    [Serializable]
    public class PostPlayerResponse : BasicResponse
    {
        /// <summary>
        /// Unique player ID assigned or used by the server.
        /// Save this for future requests to update the player entry.
        /// </summary>
        public string player_id = string.Empty;

        /// <summary>
        /// Current rank of the player after submission.
        /// </summary>
        public int rank = -1;
    }

    /// <summary>
    /// Represents pagination information for leaderboard entries.
    /// </summary>
    [Serializable]
    public class Pagination
    {
        /// <summary>
        /// Maximum number of entries returned per page.
        /// </summary>
        public int limit;

        /// <summary>
        /// Number of entries skipped before the current page.
        /// </summary>
        public int offset;

        /// <summary>
        /// Total number of entries in the leaderboard.
        /// </summary>
        public int total;

        /// <summary>
        /// Total number of pages available based on limit and total entries.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)total / limit);
    }

    /// <summary>
    /// Response containing leaderboard entries and pagination metadata.
    /// </summary>
    [Serializable]
    public class LeaderboardEntriesResponse : BasicResponse
    {
        /// <summary>
        /// Array of leaderboard entries returned.
        /// </summary>
        public LeaderboardEntry[]? entries;

        /// <summary>
        /// Pagination information for the current response.
        /// </summary>
        public Pagination? pagination;
    }

    /// <summary>
    /// Basic response format indicating success or failure and possible error message.
    /// </summary>
    [Serializable]
    public class BasicResponse
    {
        /// <summary>
        /// Indicates if the request was successful.
        /// </summary>
        public bool success;

        /// <summary>
        /// Error message if the request failed.
        /// </summary>
        public string? error;
    }

    /// <summary>
    /// Represents query parameters to fetch leaderboard entries.
    /// </summary>
    [Serializable]
    public class EntriesQuery
    {
        /// <summary>
        /// Number of entries to fetch per page.
        /// </summary>
        public int PageSize = 5;

        /// <summary>
        /// Number of entries to skip for paging.
        /// </summary>
        public int Skip = 0;

        /// <summary>
        /// Sorting order of the entries.
        /// </summary>
        public Order Order = Order.None;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntriesQuery()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="query">Existing query to copy.</param>
        public EntriesQuery(EntriesQuery query)
        {
            this.PageSize = query.PageSize;
            this.Skip = query.Skip;
            this.Order = query.Order;
        }

        /// <summary>
        /// Converts the query parameters into a dictionary for HTTP query string.
        /// </summary>
        /// <returns>Dictionary with query parameters.</returns>
        internal Dictionary<string, string> ToDictionary()
        {
            var query = new Dictionary<string, string>();

            if (PageSize > 0)
                query["limit"] = PageSize.ToString();

            if (Skip > 0)
                query["offset"] = Skip.ToString();

            if (Order == Order.Ascending)
                query["order"] = "asc";
            else if (Order == Order.Descending)
                query["order"] = "desc";

            return query;
        }

        /// <summary>
        /// Creates a new query for the next page of entries.
        /// </summary>
        /// <returns>A new <see cref="EntriesQuery"/> with offset moved forward.</returns>
        public EntriesQuery GetNextPageQuery()
        {
            return new EntriesQuery
            {
                PageSize = PageSize,
                Skip = Skip + PageSize,
                Order = Order
            };
        }

        /// <summary>
        /// Creates a new query for the previous page of entries.
        /// </summary>
        /// <returns>A new <see cref="EntriesQuery"/> with offset moved backward, not less than zero.</returns>
        public EntriesQuery GetPreviousPageQuery()
        {
            return new EntriesQuery
            {
                PageSize = PageSize,
                Skip = Math.Max(0, Skip - PageSize),
                Order = Order
            };
        }
    }

    /// <summary>
    /// Specifies sorting order for leaderboard entries.
    /// </summary>
    public enum Order
    {
        /// <summary>
        /// No sorting specified.
        /// </summary>
        None,

        /// <summary>
        /// Sort in ascending order.
        /// </summary>
        Ascending,

        /// <summary>
        /// Sort in descending order.
        /// </summary>
        Descending
    }
}

#nullable disable

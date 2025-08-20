using System;

namespace Rankora_API.Scripts.Rankora.Api
{
    /// <summary>
    /// Holds constant API route strings and helper methods to build API URLs.
    /// </summary>
    public static class Consts
    {
        // Base URL of the Rankora API.
        public const string BaseUrl = "https://www.rankora.dev/api/v1";

        // API routes for various endpoints
        public const string ApiUsageRoute = "/usage";
        public const string LeaderboardMetadataRoute = "/leaderboard";
        public const string LeaderboardEntriesRoute = "/leaderboard/entries";
        // Placeholder route for leaderboard entry by player ID
        public const string LeaderboardEntryRoute = "/leaderboard/entries/{{PLAYER_ID}}";

        /// <summary>
        /// Returns the full URL for a given API route.
        /// </summary>
        /// <param name="route">The API route enum value.</param>
        /// <param name="player_id">Optional player ID for routes that require it.</param>
        /// <returns>Full URL string to call.</returns>
        public static string GetUrl(Route route, string player_id = null)
        {
            return route switch
            {
                Route.LeaderboardMetadata => BaseUrl + LeaderboardMetadataRoute,
                Route.LeaderboardEntries => BaseUrl + LeaderboardEntriesRoute,
                Route.LeaderboardEntry => BaseUrl + GetLeaderboardEntryRoute(player_id),
                Route.Usage => BaseUrl + ApiUsageRoute,
                _ => throw new ArgumentOutOfRangeException(nameof(route), route, null)
            };
        }

        /// <summary>
        /// Builds the leaderboard entry route by replacing the player ID placeholder.
        /// Throws if player ID is null or empty.
        /// </summary>
        /// <param name="playerId">Player ID to insert into route.</param>
        /// <returns>Route string with player ID inserted.</returns>
        public static string GetLeaderboardEntryRoute(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                throw new ArgumentException("Player ID cannot be null or empty.", nameof(playerId));
            }
            return LeaderboardEntryRoute.Replace("{{PLAYER_ID}}", playerId);
        }
    }

    /// <summary>
    /// Enum representing the API routes supported by the Rankora client.
    /// </summary>
    public enum Route
    {
        LeaderboardMetadata,
        LeaderboardEntries,
        LeaderboardEntry,
        Usage,
    }
}

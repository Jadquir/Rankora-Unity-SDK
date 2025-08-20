using Assets.Rankora_API.Scripts.Rankora.Player;
using Rankora_API.Scripts.Rankora.Main;
using Rankora_API.Scripts.Rankora.Types;
using Rankora_API.Scripts.Utils.Json;
using Rankora_API.Scripts.Utils.RankoraValidation;
using Rankora_API.Scripts.Utils.Request;
using System;
using System.Linq;
using UnityEngine;
using static Rankora_API.Scripts.Rankora.Main.RankoraEvents;

namespace Rankora_API.Scripts.Rankora.Api
{
    /// <summary>
    /// Provides methods to interact with the Rankora API endpoints.
    /// Handles requests for leaderboard metadata, entries, and player entries.
    /// </summary>
    public static class RankoraClient
    {
        /// <summary>
        /// Event triggered when a global client error occurs. 
        /// Provides the error message as a string.
        /// </summary>
        public static readonly RankoraEvent<string> OnError = new(raiseWithPreviousData: false);

        /// <summary>
        /// Wraps a success callback and also raises the associated RankoraEvent.
        /// </summary>
        private static Action<T> WrapSuccessAction<T>(Action<T> onSuccess, RankoraEvent<T> eventInvoker)
        {
            return (result) =>
            {
                onSuccess?.Invoke(result);
                eventInvoker?.Raise(result);
            };
        }


        /// <summary>
        /// Handles errors by logging and invoking the error callback.
        /// </summary>
        private static void Error(Action<string> onError, string errorMessage)
        {
            if (errorMessage.StartsWith("0"))
            {
                errorMessage = "Please check your internet connection! " + $"{errorMessage.Split("0 - ").Last() ?? errorMessage}";
            }
            Debug.LogError($"[Rankora API] Error: {errorMessage}");
            onError?.Invoke(errorMessage);
            OnError?.Raise(errorMessage);
        }

        /// <summary>
        /// Fetches leaderboard metadata from the API.
        /// </summary>
        public static void GetLeaderboardMetadata(Action<LeaderboardMetadata> onSuccess = null, Action<string> onError = null)
        {
            HttpRequest.New(RequestType.GET, Consts.GetUrl(Route.LeaderboardMetadata))
                .SetSettings()?
                .SetOnSuccess<LeaderboardMetadata>(WrapSuccessAction(onSuccess, RankoraEvents.OnLeaderboardMetadataFetched))
                .SetOnError(error => Error(onError, error))
                .Send();
        }

        /// <summary>
        /// Fetches leaderboard entries based on the given query parameters.
        /// </summary>
        public static void GetLeaderboardEntries(EntriesQuery query, Action<LeaderboardEntriesResponse> onSuccess = null, Action<string> onError = null)
        {
            HttpRequest.New(RequestType.GET, Consts.GetUrl(Route.LeaderboardEntries))
                .SetSettings()?
                .SetOnSuccess<LeaderboardEntriesResponse>(WrapSuccessAction(onSuccess, RankoraEvents.OnEntriesFetched))
                .SetOnError(error => Error(onError, error))
                .SetQuery(query.ToDictionary())
                .Send();
        }

        /// <summary>
        /// Creates or updates a player entry in the leaderboard.
        /// Validates player name and metadata before sending.
        /// </summary>
        public static void CreateOrUpdatePlayerEntry(PostPlayerData playerData,
            Action<PostPlayerResponse> onSuccess = null,
            Action<string> onError = null)
        {
            var isValidName = RankoraValidation.IsValidPlayerName(playerData.name);
            if (!isValidName.IsValid)
            {
                onError?.Invoke(isValidName.ErrorMessage);
                return;
            }

            var isValidMetadata = RankoraValidation.IsValidMetadata(playerData.metadata);
            if (!isValidMetadata.IsValid)
            {
                onError?.Invoke(isValidMetadata.ErrorMessage);
                return;
            }

            HttpRequest.New(RequestType.POST, Consts.GetUrl(Route.LeaderboardEntries))
                .SetSettings()?
                .SetBody(playerData)
                .SetOnSuccess<PostPlayerResponse>(WrapSuccessAction(onSuccess, RankoraEvents.OnPostPlayerUpdated))
                .SetOnError(error => Error(onError, error))
                .Send();
        }

        /// <summary>
        /// Gets a player entry by player ID.
        /// </summary>
        public static void GetEntryById(string player_id,
            Action<PlayerEntry> onSuccess = null,
            Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(player_id))
            {
                onError?.Invoke("Player ID cannot be null or empty.");
                return;
            }

            HttpRequest.New(RequestType.GET, Consts.GetUrl(Route.LeaderboardEntry, player_id))
                .SetSettings()?
                .SetOnSuccess<PlayerEntry>(WrapSuccessAction(onSuccess, RankoraEvents.OnPlayerFetched))
                .SetOnError(error => Error(onError, error + "\n"))
                .Send();
        }

        /// <summary>
        /// Deletes a player entry by player ID.
        /// Raises OnPlayerDeleted event if successful.
        /// </summary>
        public static void DeleteEntryById(string player_id,
            Action<BasicResponse> onSuccess = null,
            Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(player_id))
            {
                onError?.Invoke("Player ID cannot be null or empty.");
                return;
            }

            HttpRequest.New(RequestType.DELETE, Consts.GetUrl(Route.LeaderboardEntry, player_id))
                .SetSettings()?
                .SetOnSuccess<BasicResponse>((res) =>
                {
                    if (res.success)
                    {
                        RankoraEvents.OnPlayerDeleted.Raise(player_id);
                        onSuccess?.Invoke(res);
                    }
                    else
                    {
                        Error(onError, res.error);
                    }
                })
                .SetOnError(error => Error(onError, error))
                .Send();
        }
    }

    /// <summary>
    /// Extension methods for HttpRequest to add common Rankora API settings.
    /// </summary>
    public static class RankoraClientExtensions
    {
#nullable enable
        static IJsonParser jsonParser = new CustomHttpJsonParser();

        /// <summary>
        /// Adds default headers, query parameters, and JSON parser to the HTTP request.
        /// Adds saved player ID as query parameter if available.
        /// Validates API key presence.
        /// </summary>
        public static HttpRequest? SetSettings(this HttpRequest request)
        {
            var instancedSettings = RankoraSettings.Instance;
            if (instancedSettings == null || string.IsNullOrEmpty(instancedSettings?.ApiKey))
            {
                Debug.LogError("[Rankora API] API Key cannot be null or empty.");
                return null;
            }

            if (RankoraPlayerId.HasSavedPlayerId())
            {
                var playerId = RankoraPlayerId.GetSavedPlayerId();
                if (!string.IsNullOrEmpty(playerId))
                {
                    request.AddQuery("player_id", playerId);
                }
            }

            return request
                .SetHeader("Authorization", $"Bearer {instancedSettings.ApiKey}")
                .SetParser(jsonParser);
        }
#nullable disable
    }
}

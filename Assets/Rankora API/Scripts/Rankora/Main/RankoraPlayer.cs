#nullable enable
using Assets.Rankora_API.Scripts.Rankora.Player;
using Rankora_API.Scripts.Rankora.Api;
using Rankora_API.Scripts.Rankora.Types;
using System;
using UnityEngine;

namespace Rankora_API.Scripts.Rankora.Main
{
    /// <summary>
    /// Represents the current player's data in Rankora and handles syncing/updating with the server.
    /// </summary>
    public class RankoraPlayer
    {
        public const string UnkownUserText = "<i>Unknown user</i>";

        /// <summary> Singleton instance of the current player. </summary>
        public static RankoraPlayer Instance { get; } = new RankoraPlayer();

        private string _playerId = string.Empty;
        private int _rank;
        private PlayerEntry? _entry;

        // Internal flags for async readiness and deferred actions
        private bool isReady = false;
        private bool isLoading = false;
        private bool requestedSync = false;
        private bool requestedUpdate = false;

        /// <summary>
        /// Raised whenever the player data is updated.
        /// </summary>
        public RankoraEvents.RankoraEvent<RankoraPlayer> OnPlayerUpdate { get; } = new();

        /// <summary>
        /// Private constructor to enforce singleton pattern.
        /// Subscribes to global player ID and rank updates.
        /// </summary>
        private RankoraPlayer()
        {
            isReady = true;

            // Listen for external player ID updates
            RankoraEvents.OnPlayerIdUpdated.Subscribe(playerId =>
            {
                if (playerId != PlayerId)
                    UpdatePlayer(playerId);
            });

            // Listen for rank changes
            RankoraEvents.OnPlayerRankUpdated.Subscribe(rank =>
            {
                _rank = rank;
                OnPlayerUpdate.Raise(this);
            });

            // Listen for player deletion to reset data
            RankoraEvents.OnPlayerDeleted.Subscribe(playerId =>
            {
                if (playerId == PlayerId)
                {
                    ResetPlayerData();
                    OnPlayerUpdate.Raise(this);
                }
            });
        }

        /// <summary>
        /// Loads and updates player info from saved PlayerId.
        /// </summary>
        public void Get()
        {
            UpdatePlayer(RankoraPlayerId.GetSavedPlayerId());
        }

        /// <summary>
        /// Starts fetching player data from the server by ID.
        /// </summary>
        private void UpdatePlayer(string playerId)
        {
            if (isLoading)
            {
                Debug.Log("Already fetching the player data.");
                return;
            }

            if (!string.IsNullOrEmpty(playerId))
            {
                _playerId = playerId;
                isReady = false;
                isLoading = true;

                RankoraClient.GetEntryById(_playerId, response =>
                {
                    isLoading = false;
                    OnGetEntryByIdSuccess(response);
                },
                errorMessage =>
                {
                    isLoading = false;
                    Debug.LogError(errorMessage);
                });
            }
            else
            {
                // No player ID — still notify listeners
                OnPlayerUpdate.Raise(this);
            }
        }

        /// <summary>
        /// Called when the player entry is successfully retrieved from the server.
        /// Updates all local player fields and raises update events.
        /// </summary>
        private void OnGetEntryByIdSuccess(PlayerEntry entry)
        {
            isReady = true;

            if (string.IsNullOrEmpty(entry.player_id))
                return;

            _entry = entry;

            _playerId = entry.player_id;
            _rank = entry.rank;
            PlayerName = entry.name;
            Score = entry.score;
            Metadata = entry.metadata ?? new Metadata();
            UpdatedAt = entry.updated_at;
            CreatedAt = entry.created_at;

            OnPlayerUpdate.Raise(this);

            // Execute deferred sync/update if requested
            if (requestedSync)
            {
                Sync();
                requestedSync = false;
            }
            if (requestedUpdate)
            {
                RefreshPlayerData();
                requestedUpdate = false;
            }
        }

        /// <summary>
        /// Resets all player data to defaults after deletion.
        /// </summary>
        private void ResetPlayerData()
        {
            _playerId = string.Empty;
            _rank = 0;
            _entry = null;
            PlayerName = UnkownUserText;
            Score = 0;
            Metadata = new Metadata();
            UpdatedAt = default;
            CreatedAt = default;
        }

        /// <summary>
        /// Returns a copy of the current player's data as a PlayerEntry.
        /// </summary>
        public PlayerEntry PlayerEntry => new()
        {
            player_id = PlayerId,
            name = PlayerName,
            score = Score,
            rank = Rank,
            metadata = Metadata,
            updated_at = UpdatedAt,
            created_at = CreatedAt,
            is_current_player = true
        };

        public string PlayerName { get; set; } = UnkownUserText;
        public double Score { get; set; }
        public int Rank => _rank;
        public string PlayerId => _playerId;
        public Metadata Metadata { get; set; } = new();
        public DateTime UpdatedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Sends updated player data to the server.
        /// If the player is not ready, the sync is deferred until ready.
        /// </summary>
        public void Sync(Action<PostPlayerResponse>? callback = null)
        {
            if (!isReady)
            {
                Debug.LogWarning("Player is not ready yet. Sync will be attempted later.");
                requestedSync = true;
                return;
            }
            
            // Avoid syncing if no changes have been made
            if (_entry != null &&
                _entry.player_id == PlayerId &&
                Equals(_entry.metadata, Metadata) &&
                _entry.score == Score &&
                _entry.name == PlayerName)
            {
                const string err = "Player data has not changed. Please update at least one field before syncing.";
                Debug.LogWarning(err);
                callback?.Invoke(new PostPlayerResponse
                {
                    success = false,
                    player_id = _entry.player_id,
                    error = err
                });
                return;
            }

            RankoraClient.CreateOrUpdatePlayerEntry(new PostPlayerData
            {
                player_id = PlayerId,
                name = PlayerName,
                score = Score,
                metadata = Metadata
            },
            response =>
            {
                Debug.Log("Player data synced successfully.");
                callback?.Invoke(response);
            },
            error =>
            {
                Debug.LogError($"Failed to sync player data: {error}");
                callback?.Invoke(new PostPlayerResponse
                {
                    success = false,
                    player_id = string.Empty,
                    error = error
                });
            });
        }

        /// <summary>
        /// Fetches the latest player data from the server.
        /// If the player is not ready, the update is deferred until ready.
        /// </summary>
        public void RefreshPlayerData()
        {
            if (!isReady)
            {
                Debug.LogWarning("Player is not ready yet. Update will be attempted later.");
                requestedUpdate = true;
                return;
            }

            if (string.IsNullOrEmpty(PlayerId))
            {
                Debug.LogError("Player ID is not set. Cannot update player.");
                return;
            }

            RankoraClient.GetEntryById(PlayerId, OnGetEntryByIdSuccess, error =>
            {
                Debug.LogError($"Failed to update player: {error}");
            });
        }

        /// <summary>
        /// Deletes the current player's leaderboard entry from the server.
        /// 
        /// <b>Warning:</b> This action is irreversible. Once deleted, the player's data cannot be recovered.
        /// </summary>
        public void DeleteCurrentPlayer()
        {
            if (!isReady)
            {
                Debug.LogWarning("Player is not ready yet. Delete will be attempted later.");
                requestedUpdate = true;
                return;
            }

            if (string.IsNullOrEmpty(PlayerId))
            {
                Debug.LogError("Player ID is not set. Cannot delete player.");
                return;
            }

            RankoraClient.DeleteEntryById(PlayerId);
        }
    }
}
#nullable disable

using Rankora_API.Scripts.Rankora.Api;
using Rankora_API.Scripts.Rankora.Main;
using Rankora_API.Scripts.Rankora.Types;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Rankora_API.Scripts.Rankora.Player
{
    /// <summary>
    /// Static utility class for managing saving and retrieving the player's unique ID.
    /// Supports multiple storage methods based on RankoraSettings configuration.
    /// </summary>
    public static class RankoraPlayerId
    {
        // Cached in-memory player ID to avoid repeated IO operations
        static string _playerId = null;

        static RankoraPlayerId()
        {
            // Automatically update saved player ID whenever RankoraEvents signals an update
            RankoraEvents.OnPlayerIdUpdated.Subscribe(SetSavedPlayerId);
        }

        // Constants for PlayerPrefs key and persistent file name
        const string PlayerPrefsKey = "RankoraPlayerId";
        const string SaveFileName = "rankora_player_id.txt";

        // Path for persistent storage in file system
        static readonly string SaveFolder = Path.Combine(Application.persistentDataPath, Application.productName, "Rankora");
        static readonly string SavePath = Path.Combine(SaveFolder, SaveFileName);

        /// <summary>
        /// Checks if there is a saved player ID currently available.
        /// </summary>
        public static bool HasSavedPlayerId()
        {
            if (!string.IsNullOrEmpty(_playerId))
            {
                return true;
            }
            GetSavedPlayerId(); // Try to load if not cached

            return !string.IsNullOrEmpty(_playerId);
        }

        /// <summary>
        /// Retrieves the saved player ID from memory or storage.
        /// </summary>
        public static string GetSavedPlayerId()
        {
            if (!string.IsNullOrEmpty(_playerId))
            {
                return _playerId;
            }

            _playerId = string.Empty;

            switch (RankoraSettings.Instance.PlayerIdSaveMode)
            {
                case RankoraSettings.PlayerIdSaveModeType.PlayerPrefs:
                    if (PlayerPrefs.HasKey(PlayerPrefsKey))
                    {
                        _playerId = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
                    }
                    break;

                case RankoraSettings.PlayerIdSaveModeType.PersistentDataPath:
                    try
                    {
                        if (File.Exists(SavePath))
                        {
                            _playerId = File.ReadAllText(SavePath);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to read player ID from file: {e.Message}");
                    }
                    break;

                case RankoraSettings.PlayerIdSaveModeType.Unhandled:
                default:
                    // No saving/loading handled
                    break;
            }

            return _playerId;
        }

        /// <summary>
        /// Saves or updates the player ID in memory and persistent storage based on settings.
        /// </summary>
        /// <param name="playerId">The player ID string to save.</param>
        public static void SetSavedPlayerId(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return; // Ignore invalid input
            }

            // If the ID is already saved, no action needed
            if (GetSavedPlayerId() == playerId)
            {
                return;
            }

            Debug.Log($"Setting saved player ID: {playerId}");

            _playerId = playerId;

            switch (RankoraSettings.Instance.PlayerIdSaveMode)
            {
                case RankoraSettings.PlayerIdSaveModeType.PlayerPrefs:
                    PlayerPrefs.SetString(PlayerPrefsKey, _playerId);
                    PlayerPrefs.Save();
                    break;

                case RankoraSettings.PlayerIdSaveModeType.PersistentDataPath:
                    try
                    {
                        if (!Directory.Exists(SaveFolder))
                        {
                            Directory.CreateDirectory(SaveFolder);
                        }
                        File.WriteAllText(SavePath, _playerId);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to write player ID to file: {e.Message}");
                    }
                    break;

                case RankoraSettings.PlayerIdSaveModeType.Unhandled:
                default:
                    // Do nothing
                    break;
            }
        }

        /// <summary>
        /// Clears the saved player ID from memory and persistent storage.
        /// </summary>
        public static void ClearSavedPlayerId()
        {
            _playerId = null;

            switch (RankoraSettings.Instance.PlayerIdSaveMode)
            {
                case RankoraSettings.PlayerIdSaveModeType.PlayerPrefs:
                    PlayerPrefs.DeleteKey(PlayerPrefsKey);
                    PlayerPrefs.Save();
                    break;

                case RankoraSettings.PlayerIdSaveModeType.PersistentDataPath:
                    try
                    {
                        if (File.Exists(SavePath))
                            File.Delete(SavePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to delete player ID file: {e.Message}");
                    }
                    break;

                case RankoraSettings.PlayerIdSaveModeType.Unhandled:
                default:
                    // No cleanup handled
                    break;
            }
        }
    }
}

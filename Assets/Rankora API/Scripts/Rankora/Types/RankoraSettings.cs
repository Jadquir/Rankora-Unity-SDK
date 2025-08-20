using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rankora_API.Scripts.Rankora.Types
{
    /// <summary>
    /// Singleton ScriptableObject that stores configuration settings for Rankora API usage.
    /// Includes API key, leaderboard display settings, player ID storage preferences, and UI styles.
    /// </summary>
    [Icon("Assets/Rankora API/Sprites/ScriptIcons/Settings.png")]
    public class RankoraSettings : ScriptableObject
    {
        /// <summary>
        /// Defines where the player ID is saved.
        /// </summary>
        public enum PlayerIdSaveModeType
        {
            PlayerPrefs,        // Saved in Unity's PlayerPrefs
            PersistentDataPath, // Saved in a file under Application.persistentDataPath
            Unhandled           // No saving handled
        }

        [SerializeField]
        private string apiKey;
        /// <summary>
        /// API key used to authenticate Rankora API requests.
        /// </summary>
        public string ApiKey { get { return apiKey; } set { apiKey = value; } }

        [SerializeField]
        private PlayerIdSaveModeType playerIdSaveMode;
        /// <summary>
        /// Gets the current mode for saving player ID.
        /// </summary>
        public PlayerIdSaveModeType PlayerIdSaveMode { get { return playerIdSaveMode; } }

        /// <summary>
        /// Defines how scores are formatted for display.
        /// </summary>
        public enum ScoreFormatType
        {
            Integer,
            Float,
            Time
        }

        [Header("Leaderboard")]
        [Tooltip("If set to None, Rankora will use the default sorting defined in the leaderboard settings. You can change this default in your Rankora dashboard.")]
        [SerializeField]
        private Order leaderboardSorting;
        /// <summary>
        /// Sorting order for the leaderboard entries.
        /// </summary>
        public Order LeaderboardSorting => leaderboardSorting;

        [Tooltip("The number of entries to fetch from the leaderboard.")]
        [SerializeField]
        private int pageSize;
        /// <summary>
        /// Number of leaderboard entries fetched per page.
        /// </summary>
        public int PageSize => pageSize;

        [Header("Other")]
        [SerializeField]
        private ScoreFormatType scoreFormat;
        /// <summary>
        /// Format type used for displaying scores.
        /// </summary>
        public ScoreFormatType ScoreFormat => scoreFormat;

        [SerializeField]
        private string leaderboardEmptyText = "No entries found! Try submitting a score";
        /// <summary>
        /// Text shown when the leaderboard is empty.
        /// </summary>
        public string LeaderboardEmptyText => leaderboardEmptyText;

        [SerializeField]
        private string loadingLeaderboardText = "Loading Leaderboard...";
        /// <summary>
        /// Text shown while leaderboard data is loading.
        /// </summary>
        public string LoadingLeaderboardText => loadingLeaderboardText;

        [SerializeField]
        private string currentPlayerTextFormat = "{0} (You)";
        /// <summary>
        /// Format string for showing the current player's name.
        /// </summary>
        public string CurrentPlayerTextFormat => currentPlayerTextFormat;
        [SerializeField]
        private string currentPageFormat = "Page: {0}";
        /// <summary>
        /// Format string for showing the page number.
        /// </summary>
        public string CurrentPageFormat => currentPageFormat;

        /// <summary>
        /// Reference to EntryStyle ScriptableObject assets for leaderboard entry styling.
        /// </summary>
        [SerializeField]
        private List<EntryStyle> entryStyleAssets = new List<EntryStyle>();
        
        /// <summary>
        /// Current list of entry style assets used for leaderboard entries.
        /// </summary>
        public List<EntryStyle> EntryStyleAssets => entryStyleAssets;

        /// <summary>
        /// Loads default entry styles from the EntryStyles folder or generates them if they don't exist.
        /// </summary>
        [ContextMenu("Load Default Styles")]
        public void LoadDefaultStyles()
        {
            var defaultStyles = Resources.LoadAll<EntryStyle>("Rankora API/EntryStyles");
            if (defaultStyles.Length > 0)
            {
                entryStyleAssets = new List<EntryStyle>(defaultStyles);
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
                Debug.Log("[Rankora] Loaded default entry styles from Resources");
            }
            else
            {
#if UNITY_EDITOR
                // Auto-generate default styles if none exist
                Debug.Log("[Rankora] No default styles found. Auto-generating default styles...");
                var styleGenerator = System.Type.GetType("Rankora_API.Editor.RankoraEntryStyleGenerator");
                if (styleGenerator != null)
                {
                    var generateMethod = styleGenerator.GetMethod("GenerateDefaultStyles", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (generateMethod != null)
                    {
                        generateMethod.Invoke(null, null);
                        
                        // Try to load the generated styles
                        defaultStyles = Resources.LoadAll<EntryStyle>("Rankora API/EntryStyles");
                        if (defaultStyles.Length > 0)
                        {
                            entryStyleAssets = new List<EntryStyle>(defaultStyles);
                            EditorUtility.SetDirty(this);
                            Debug.Log("[Rankora] Auto-generated and loaded default entry styles");
                        }
                    }
                }
#endif
                if (entryStyleAssets.Count == 0)
                {
                    Debug.LogWarning("[Rankora] Could not auto-generate default styles. Use 'Generate Default Entry Styles' from the menu.");
                }
            }
        }

        /// <summary>
        /// Gets the entry styles as EntryStyleData for runtime use.
        /// </summary>
        public List<EntryStyleData> GetEntryStyleData()
        {
            var styleData = new List<EntryStyleData>();
            foreach (var styleAsset in entryStyleAssets)
            {
                if (styleAsset != null)
                {
                    styleData.Add(styleAsset.Clone());
                }
            }
            return styleData;
        }

        /// <summary>
        /// Checks if default styles are currently loaded.
        /// </summary>
        public bool HasDefaultStyles()
        {
            return entryStyleAssets.Count >= 3 && 
                   entryStyleAssets[0] != null && 
                   entryStyleAssets[1] != null && 
                   entryStyleAssets[2] != null;
        }

        /// <summary>
        /// Forces the loading of default styles, creating them if necessary.
        /// </summary>
        [ContextMenu("Force Load Default Styles")]
        public void ForceLoadDefaultStyles()
        {
#if UNITY_EDITOR
            // Clear existing styles
            entryStyleAssets.Clear();
            
            // Try to load existing styles first
            var defaultStyles = Resources.LoadAll<EntryStyle>("Rankora API/EntryStyles");
            if (defaultStyles.Length == 0)
            {
                // Generate default styles if none exist
                var styleGenerator = System.Type.GetType("Rankora_API.Editor.RankoraEntryStyleGenerator");
                if (styleGenerator != null)
                {
                    var generateMethod = styleGenerator.GetMethod("GenerateDefaultStyles", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (generateMethod != null)
                    {
                        generateMethod.Invoke(null, null);
                        defaultStyles = Resources.LoadAll<EntryStyle>("Rankora API/EntryStyles");
                    }
                }
            }
            
            // Assign the styles
            if (defaultStyles.Length > 0)
            {
                entryStyleAssets.AddRange(defaultStyles);
                EditorUtility.SetDirty(this);
                Debug.Log("[Rankora] Force loaded default styles");
            }
            else
            {
                Debug.LogError("[Rankora] Failed to load or generate default styles");
            }
#endif
        }

        private const string ResourceSubfolder = "Resources/Rankora API";
        private const string FileName = "RankoraSettings";
        private const string ResourcePath = "Rankora API/RankoraSettings";

        private static RankoraSettings _instance;

        /// <summary>
        /// Singleton instance accessor for the settings.
        /// Loads or creates the settings asset if necessary.
        /// </summary>
        public static RankoraSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = LoadOrCreateSettings();
                return _instance;
            }
        }

        /// <summary>
        /// Loads settings from Resources or creates a new asset if not found (Editor only).
        /// </summary>
        private static RankoraSettings LoadOrCreateSettings()
        {
            var settings = Resources.Load<RankoraSettings>(ResourcePath);

#if UNITY_EDITOR
            if (settings == null)
            {
                settings = CreateInstance<RankoraSettings>();

                string fullFolderPath = Path.Combine("Assets", ResourceSubfolder);
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");

                if (!AssetDatabase.IsValidFolder(fullFolderPath))
                {
                    var parent = "Assets/Resources";
                    var leaf = "Rankora API";
                    AssetDatabase.CreateFolder(parent, leaf);
                }

                string assetPath = Path.Combine("Assets", ResourceSubfolder, FileName + ".asset");
                AssetDatabase.CreateAsset(settings, assetPath);
                
                // Auto-load default styles when creating new settings
                settings.LoadDefaultStyles();
                
                AssetDatabase.SaveAssets();
                Debug.Log($"[Rankora] Created settings at: {assetPath} with default styles loaded");
            }
#endif
            return settings;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Adds a menu item to open the RankoraSettings asset in the editor.
        /// </summary>
        [MenuItem("Rankora API/Open Settings")]
        private static void OpenSettingsAsset()
        {
            Selection.activeObject = Instance;
        }
#endif
    }
}

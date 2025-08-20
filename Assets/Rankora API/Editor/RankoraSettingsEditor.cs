#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Rankora_API.Scripts.Rankora.Api;
using Rankora_API.Scripts.Rankora.Types;

namespace Rankora_API.Editor
{
    /// <summary>
    /// Custom editor for RankoraSettings that provides a clean, organized interface.
    /// </summary>
    [CustomEditor(typeof(RankoraSettings))]
    public class RankoraSettingsEditor : UnityEditor.Editor
    {
        private bool showApiConfig = true;
        private bool showLeaderboard = true;
        private bool showUIStrings = true;
        private bool showEntryStyles = true;
        private bool showApiKey = false;

        private SerializedProperty apiKeyProp;
        private SerializedProperty playerIdSaveModeProp;
        private SerializedProperty leaderboardSortingProp;
        private SerializedProperty pageSizeProp;
        private SerializedProperty scoreFormatProp;
        private SerializedProperty leaderboardEmptyTextProp;
        private SerializedProperty loadingLeaderboardTextProp;
        private SerializedProperty currentPlayerTextFormatProp;
        private SerializedProperty currentPageFormatProp;
        //private SerializedProperty entryStylesProp;
        private void OnEnable()
        {
            apiKeyProp = serializedObject.FindProperty("apiKey");
            playerIdSaveModeProp = serializedObject.FindProperty("playerIdSaveMode");
            leaderboardSortingProp = serializedObject.FindProperty("leaderboardSorting");
            pageSizeProp = serializedObject.FindProperty("pageSize");
            scoreFormatProp = serializedObject.FindProperty("scoreFormat");
            leaderboardEmptyTextProp = serializedObject.FindProperty("leaderboardEmptyText");
            loadingLeaderboardTextProp = serializedObject.FindProperty("loadingLeaderboardText");
            currentPlayerTextFormatProp = serializedObject.FindProperty("currentPlayerTextFormat");
            currentPageFormatProp = serializedObject.FindProperty("currentPageFormat");
            //entryStylesProp = serializedObject.FindProperty("entryStyleAssets");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space();

            DrawApiConfiguration();
            EditorGUILayout.Space();

            DrawLeaderboardSettings();
            EditorGUILayout.Space();

            DrawUIStrings();
            EditorGUILayout.Space();

            DrawEntryStyles();
            EditorGUILayout.Space();

            DrawValidationAndHelp();
            EditorGUILayout.Space();

            DrawActionButtons();

            serializedObject.ApplyModifiedProperties();
        }

        private new void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Rankora API Settings", EditorStyles.largeLabel, GUILayout.Height(24));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Configure your Rankora API integration settings. Make sure to set your API key from the Rankora dashboard.",
                MessageType.Info);
        }

        private void DrawApiConfiguration()
        {
            showApiConfig = EditorGUILayout.Foldout(showApiConfig, "API Configuration", true);
            if (showApiConfig)
            {
                EditorGUI.indentLevel++;
                
                // API Key with show/hide toggle
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("API Key", "Your Rankora API key from the dashboard"), GUILayout.Width(EditorGUIUtility.labelWidth));
                
                // Show/hide toggle button
                if (GUILayout.Button(showApiKey ? "Hide" : "Show", GUILayout.Width(50)))
                {
                    showApiKey = !showApiKey;
                }
                
                EditorGUILayout.EndHorizontal();
                
                // API Key value field
                if (showApiKey)
                {
                    EditorGUILayout.PropertyField(apiKeyProp, GUIContent.none);
                }
                else
                {
                    // Display dots for hidden API key
                    string apiKeyValue = apiKeyProp.stringValue ?? "";
                    string hiddenKey = string.IsNullOrEmpty(apiKeyValue) ? "No API Key Set" : new string('•', Mathf.Min(apiKeyValue.Length, 20));

                    
                    EditorGUILayout.LabelField(hiddenKey, EditorStyles.textField);
                }
                
                if (string.IsNullOrEmpty(apiKeyProp.stringValue))
                {
                    EditorGUILayout.HelpBox("API Key is required! Get it from the Rankora dashboard.", MessageType.Warning);
                    if (GUILayout.Button("Open Rankora Dashboard"))
                    {
                        Application.OpenURL("https://rankora.dev/dashboard");
                    }
                }
                else if (apiKeyProp.stringValue.Length < 10)
                {
                    EditorGUILayout.HelpBox("API Key seems too short. Please verify it's correct.", MessageType.Warning);
                }

                EditorGUILayout.PropertyField(playerIdSaveModeProp, new GUIContent("Player ID Save Mode", "How player IDs are persisted"));
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawLeaderboardSettings()
        {
            showLeaderboard = EditorGUILayout.Foldout(showLeaderboard, "Leaderboard Settings", true);
            if (showLeaderboard)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(leaderboardSortingProp, new GUIContent("Sorting Order", "Default sorting for leaderboard entries"));
                EditorGUILayout.PropertyField(pageSizeProp, new GUIContent("Page Size", "Number of entries per page"));
                EditorGUILayout.PropertyField(scoreFormatProp, new GUIContent("Score Format", "How scores are displayed"));
                
                if (pageSizeProp.intValue <= 0)
                {
                    pageSizeProp.intValue = 10;
                    EditorGUILayout.HelpBox("Page size must be greater than 0. Set to 10.", MessageType.Warning);
                }
                else if (pageSizeProp.intValue > 100)
                {
                    EditorGUILayout.HelpBox("Large page sizes may impact performance. Consider using pagination.", MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawUIStrings()
        {
            showUIStrings = EditorGUILayout.Foldout(showUIStrings, "UI Text Strings", true);
            if (showUIStrings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(leaderboardEmptyTextProp, new GUIContent("Empty Leaderboard Text", "Text shown when no entries exist"));
                EditorGUILayout.PropertyField(loadingLeaderboardTextProp, new GUIContent("Loading Text", "Text shown while loading data"));
                EditorGUILayout.PropertyField(currentPlayerTextFormatProp, new GUIContent("Current Player Format", "Format string for current player display"));
                EditorGUILayout.PropertyField(currentPageFormatProp, new GUIContent("Current Page Format", "Format string for current page display"));
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawEntryStyles()
        {
            showEntryStyles = EditorGUILayout.Foldout(showEntryStyles, "Entry Styles", true);
            if (showEntryStyles)
            {
                EditorGUI.indentLevel++;

                // Header with info
                EditorGUILayout.HelpBox(
                    "Entry Styles define the visual appearance of leaderboard entries. " +
                    "Styles are applied in order: 1st style for 1st place, 2nd for 2nd place, etc.",
                    MessageType.Info);

                // Get the current list
                var settings = (RankoraSettings)target;
                var currentStyles = settings.EntryStyleAssets;
                
                // Display current styles with reorderable list
                for (int i = 0; i < currentStyles.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // Style slot label
                    EditorGUILayout.LabelField($"Rank {i + 1}", GUILayout.Width(60));
                    
                    // Style asset field
                    var newStyle = (EntryStyle)EditorGUILayout.ObjectField(
                        currentStyles[i], 
                        typeof(EntryStyle), 
                        false,
                        GUILayout.ExpandWidth(true));
                    
                    // Update the list if changed
                    if (newStyle != currentStyles[i])
                    {
                        Undo.RecordObject(settings, "Change Entry Style");
                        currentStyles[i] = newStyle;
                        EditorUtility.SetDirty(settings);
                    }
                    
                    // Remove button
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(settings, "Remove Entry Style");
                        currentStyles.RemoveAt(i);
                        EditorUtility.SetDirty(settings);
                        break; // Exit loop since we modified the collection
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                // Add new style button
                if (GUILayout.Button("Add New Style Slot", GUILayout.Height(24)))
                {
                    Undo.RecordObject(settings, "Add Entry Style Slot");
                    currentStyles.Add(null);
                    EditorUtility.SetDirty(settings);
                }
                
                // Clear all styles button
                if (currentStyles.Count > 0 && GUILayout.Button("Clear All Styles", GUILayout.Height(20)))
                {
                    if (EditorUtility.DisplayDialog("Clear All Styles", 
                        "Are you sure you want to remove all entry styles?", "Clear", "Cancel"))
                    {
                        Undo.RecordObject(settings, "Clear All Entry Styles");
                        currentStyles.Clear();
                        EditorUtility.SetDirty(settings);
                    }
                }
                
                EditorGUILayout.Space();
                
                // Quick actions section
                EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                //if (GUILayout.Button("Generate Default Styles"))
                //{
                //    if (EditorUtility.DisplayDialog("Generate Default Styles", 
                //        "This will create Gold, Silver, and Bronze styles and assign them to the first 3 slots. Continue?", "Generate", "Cancel"))
                //    {
                //        RankoraEntryStyleGenerator.GenerateDefaultStyles();
                        
                //        // Auto-assign the generated styles
                //        var generatedStyles = RankoraEntryStyleGenerator.GetGeneratedStyles();
                //        if (generatedStyles.Count > 0)
                //        {
                //            Undo.RecordObject(settings, "Assign Generated Styles");
                            
                //            // Clear existing and add new
                //            currentStyles.Clear();
                //            foreach (var style in generatedStyles)
                //            {
                //                currentStyles.Add(style);
                //            }
                            
                //            EditorUtility.SetDirty(settings);
                //            Debug.Log("[Rankora] Generated and assigned default styles automatically");
                //        }
                //    }
                //}
                
                if (GUILayout.Button("Set Default Styles"))
                {
                    var defaultStyles = RankoraEntryStyleGenerator.GetGeneratedStyles();
                    if (defaultStyles.Count > 0)
                    {
                        if (EditorUtility.DisplayDialog("Set Default Styles", 
                            "This will assign the existing Gold, Silver, and Bronze styles to the first 3 slots. Continue?", "Set", "Cancel"))
                        {
                            Undo.RecordObject(settings, "Set Default Styles");
                            
                            // Clear existing and add default styles
                            currentStyles.Clear();
                            foreach (var style in defaultStyles)
                            {
                                currentStyles.Add(style);
                            }
                            
                            EditorUtility.SetDirty(settings);
                            Debug.Log("[Rankora] Set default styles: Gold, Silver, Bronze");
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("No Default Styles Found", 
                            "Default styles (Gold, Silver, Bronze) were not found. Please use 'Generate Default Styles' first to create them.", "OK");
                    }
                }
                
                if (GUILayout.Button("Open Styles Folder"))
                {
                    RankoraEntryStyleGenerator.OpenEntryStylesFolder();
                }
                
                if (GUILayout.Button("Force Load Default Styles"))
                {
                    if (EditorUtility.DisplayDialog("Force Load Default Styles", 
                        "This will clear all current styles and force load the default Gold, Silver, and Bronze styles. Continue?", "Load", "Cancel"))
                    {
                        Undo.RecordObject(settings, "Force Load Default Styles");
                        settings.ForceLoadDefaultStyles();
                        EditorUtility.SetDirty(settings);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                // Style preview info
                if (currentStyles.Count > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(
                        $"Currently assigned: {currentStyles.Count} style(s)\n" +
                        $"1st place: {(currentStyles.Count > 0 && currentStyles[0] != null ? currentStyles[0].name : "None")}\n" +
                        $"2nd place: {(currentStyles.Count > 1 && currentStyles[1] != null ? currentStyles[1].name : "None")}\n" +
                        $"3rd place: {(currentStyles.Count > 2 && currentStyles[2] != null ? currentStyles[2].name : "None")}",
                        MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
        }


        private void DrawValidationAndHelp()
        {
            var settings = (RankoraSettings)target;
            
            if (string.IsNullOrEmpty(settings.ApiKey))
            {
                EditorGUILayout.HelpBox(
                    "⚠️ Configuration Incomplete\n\n" +
                    "• API Key is missing\n" +
                    "• Get your API key from: https://rankora.dev/dashboard\n" +
                    "• Set it above to enable Rankora functionality",
                    MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "✅ Configuration Complete\n\n" +
                    "Your Rankora API is properly configured and ready to use!",
                    MessageType.Info);
            }
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Open Rankora Dashboard", GUILayout.Height(24)))
            {
                Application.OpenURL("https://rankora.dev/dashboard");
            }
            
            if (GUILayout.Button("Documentation", GUILayout.Height(24)))
            {
                var docPath = "Assets/Rankora API/Rankora_Unity_SDK_Documentation.md";
                if (System.IO.File.Exists(docPath))
                {
                    var docAsset = AssetDatabase.LoadAssetAtPath<Object>(docPath);
                    Selection.activeObject = docAsset;
                    EditorGUIUtility.PingObject(docAsset);
                }
                else
                {
                    EditorUtility.DisplayDialog("Documentation Not Found", 
                        "Could not find the documentation file. Please ensure the SDK is imported correctly.", "OK");
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset to Defaults", GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("Reset Settings", 
                    "Are you sure you want to reset all settings to their default values?", 
                    "Reset", "Cancel"))
                {
                    ResetToDefaults();
                }
            }
            
            if (GUILayout.Button("Validate Settings", GUILayout.Height(20)))
            {
                ValidateSettings();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void ResetToDefaults()
        {
            var settings = (RankoraSettings)target;
            
            Undo.RecordObject(settings, "Reset Rankora Settings");
            
            settings.ApiKey = "";
            settings.ForceLoadDefaultStyles();
            
            EditorUtility.SetDirty(settings);
            Debug.Log("[Rankora] Settings reset to defaults");
        }

        private void ValidateSettings()
        {
            var settings = (RankoraSettings)target;
            var issues = new System.Collections.Generic.List<string>();
            
            if (string.IsNullOrEmpty(settings.ApiKey))
                issues.Add("API Key is missing");
            
            if (settings.PageSize <= 0)
                issues.Add("Page size must be greater than 0");
            
            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Passed", 
                    "All settings are valid and properly configured!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Issues Found", 
                    "The following issues were found:\n\n• " + string.Join("\n• ", issues), "OK");
            }
        }
    }
}
#endif

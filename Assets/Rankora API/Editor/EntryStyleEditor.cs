#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Rankora_API.Scripts.Rankora.Types;

namespace Rankora_API.Editor
{
    /// <summary>
    /// Custom editor for EntryStyle ScriptableObject that provides a clean, organized interface.
    /// </summary>
    [CustomEditor(typeof(EntryStyle))]
    public class EntryStyleEditor : UnityEditor.Editor
    {
        private bool showEntryStyle = true;
        private bool showRankStyle = true;
        private bool showPreview = true;

        private SerializedProperty backgroundColorProp;
        private SerializedProperty outlineColorProp;
        private SerializedProperty outlineDistanceProp;
        private SerializedProperty textColorProp;
        private SerializedProperty rankBackgroundColorProp;
        private SerializedProperty rankOutlineColorProp;
        private SerializedProperty rankOutlineDistanceProp;
        private SerializedProperty rankTextColorProp;

        private void OnEnable()
        {
            backgroundColorProp = serializedObject.FindProperty("backgroundColor");
            outlineColorProp = serializedObject.FindProperty("outlineColor");
            outlineDistanceProp = serializedObject.FindProperty("outlineDistance");
            textColorProp = serializedObject.FindProperty("textColor");
            rankBackgroundColorProp = serializedObject.FindProperty("rankBackgroundColor");
            rankOutlineColorProp = serializedObject.FindProperty("rankOutlineColor");
            rankOutlineDistanceProp = serializedObject.FindProperty("rankOutlineDistance");
            rankTextColorProp = serializedObject.FindProperty("rankTextColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space();

            DrawEntryStyleSection();
            EditorGUILayout.Space();

            DrawRankStyleSection();
            EditorGUILayout.Space();

            DrawPreviewSection();
            EditorGUILayout.Space();

            DrawActionButtons();

            serializedObject.ApplyModifiedProperties();
        }

        private new void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Entry Style", EditorStyles.largeLabel, GUILayout.Height(24));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            var style = (EntryStyle)target;
            EditorGUILayout.HelpBox(
                $"Style: {style.name}\nConfigure the visual appearance of leaderboard entries and rank numbers.",
                MessageType.Info);
        }

        private void DrawEntryStyleSection()
        {
            showEntryStyle = EditorGUILayout.Foldout(showEntryStyle, "Entry Style", true);
            if (showEntryStyle)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(backgroundColorProp, new GUIContent("Background Color", "Background color for the entry"));
                EditorGUILayout.PropertyField(outlineColorProp, new GUIContent("Outline Color", "Outline color for the entry"));
                EditorGUILayout.PropertyField(outlineDistanceProp, new GUIContent("Outline Distance", "Outline offset/distance from the entry"));
                EditorGUILayout.PropertyField(textColorProp, new GUIContent("Text Color", "Color for the entry text"));
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawRankStyleSection()
        {
            showRankStyle = EditorGUILayout.Foldout(showRankStyle, "Rank Style", true);
            if (showRankStyle)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(rankBackgroundColorProp, new GUIContent("Background Color", "Background color for the rank number"));
                EditorGUILayout.PropertyField(rankOutlineColorProp, new GUIContent("Outline Color", "Outline color for the rank number"));
                EditorGUILayout.PropertyField(rankOutlineDistanceProp, new GUIContent("Outline Distance", "Outline offset/distance from the rank number"));
                EditorGUILayout.PropertyField(rankTextColorProp, new GUIContent("Text Color", "Color for the rank number text"));
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawPreviewSection()
        {
            showPreview = EditorGUILayout.Foldout(showPreview, "Style Preview", true);
            if (showPreview)
            {
                EditorGUI.indentLevel++;
                
                var style = (EntryStyle)target;
                
                // Entry preview
                EditorGUILayout.LabelField("Entry Preview", EditorStyles.boldLabel);
                DrawColorPreview("Background", style.backgroundColor);
                DrawColorPreview("Outline", style.outlineColor);
                DrawColorPreview("Text", style.textColor);
                EditorGUILayout.LabelField("Outline Distance", $"{style.outlineDistance.x:F2}, {style.outlineDistance.y:F2}");
                
                EditorGUILayout.Space();
                
                // Rank preview
                EditorGUILayout.LabelField("Rank Preview", EditorStyles.boldLabel);
                DrawColorPreview("Background", style.rankBackgroundColor);
                DrawColorPreview("Outline", style.rankOutlineColor);
                DrawColorPreview("Text", style.rankTextColor);
                EditorGUILayout.LabelField("Outline Distance", $"{style.rankOutlineDistance.x:F2}, {style.rankOutlineDistance.y:F2}");
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawColorPreview(string label, Color color)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(80));
            
            // Color preview box
            var rect = GUILayoutUtility.GetRect(20, 20);
            EditorGUI.DrawRect(rect, color);
            
            // Color values
            EditorGUILayout.LabelField($"RGB({color.r:F2}, {color.g:F2}, {color.b:F2})", GUILayout.Width(120));
            EditorGUILayout.LabelField($"A({color.a:F2})", GUILayout.Width(60));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset to Default", GUILayout.Height(24)))
            {
                if (EditorUtility.DisplayDialog("Reset Style", 
                    "Are you sure you want to reset this style to default values?", 
                    "Reset", "Cancel"))
                {
                    ResetToDefault();
                }
            }
            
            if (GUILayout.Button("Duplicate Style", GUILayout.Height(24)))
            {
                DuplicateStyle();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Copy Colors", GUILayout.Height(20)))
            {
                CopyColorsToClipboard();
            }
            
            if (GUILayout.Button("Paste Colors", GUILayout.Height(20)))
            {
                PasteColorsFromClipboard();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void ResetToDefault()
        {
            var style = (EntryStyle)target;
            
            Undo.RecordObject(style, "Reset Entry Style");
            style.ResetToDefault();
            
            EditorUtility.SetDirty(style);
            Debug.Log($"[Rankora] Reset {style.name} to default values");
        }

        private void DuplicateStyle()
        {
            var originalStyle = (EntryStyle)target;
            var newStyle = CreateInstance<EntryStyle>();
            
            // Copy all properties
            newStyle.backgroundColor = originalStyle.backgroundColor;
            newStyle.outlineColor = originalStyle.outlineColor;
            newStyle.outlineDistance = originalStyle.outlineDistance;
            newStyle.textColor = originalStyle.textColor;
            newStyle.rankBackgroundColor = originalStyle.rankBackgroundColor;
            newStyle.rankOutlineColor = originalStyle.rankOutlineColor;
            newStyle.rankOutlineDistance = originalStyle.rankOutlineDistance;
            newStyle.rankTextColor = originalStyle.rankTextColor;
            
            // Generate unique name
            string baseName = originalStyle.name.Replace("EntryStyle", "");
            string newName = $"{baseName}EntryStyle_Copy";
            
            // Create asset
            string path = AssetDatabase.GetAssetPath(originalStyle);
            string directory = System.IO.Path.GetDirectoryName(path);
            string newPath = System.IO.Path.Combine(directory, $"{newName}.asset");
            
            AssetDatabase.CreateAsset(newStyle, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the new asset
            Selection.activeObject = newStyle;
            EditorGUIUtility.PingObject(newStyle);
            
            Debug.Log($"[Rankora] Duplicated style to: {newPath}");
        }

        private void CopyColorsToClipboard()
        {
            var style = (EntryStyle)target;
            var colorData = new
            {
                backgroundColor = style.backgroundColor,
                outlineColor = style.outlineColor,
                textColor = style.textColor,
                rankBackgroundColor = style.rankBackgroundColor,
                rankOutlineColor = style.rankOutlineColor,
                rankTextColor = style.rankTextColor
            };
            
            string json = JsonUtility.ToJson(colorData, true);
            EditorGUIUtility.systemCopyBuffer = json;
            
            Debug.Log("[Rankora] Colors copied to clipboard");
        }

        private void PasteColorsFromClipboard()
        {
            try
            {
                string json = EditorGUIUtility.systemCopyBuffer;
                var colorData = JsonUtility.FromJson<ColorData>(json);
                
                if (colorData != null)
                {
                    var style = (EntryStyle)target;
                    
                    Undo.RecordObject(style, "Paste Colors");
                    
                    style.backgroundColor = colorData.backgroundColor;
                    style.outlineColor = colorData.outlineColor;
                    style.textColor = colorData.textColor;
                    style.rankBackgroundColor = colorData.rankBackgroundColor;
                    style.rankOutlineColor = colorData.rankOutlineColor;
                    style.rankTextColor = colorData.rankTextColor;
                    
                    EditorUtility.SetDirty(style);
                    Debug.Log("[Rankora] Colors pasted from clipboard");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Rankora] Failed to paste colors: {e.Message}");
            }
        }

        [System.Serializable]
        private class ColorData
        {
            public Color backgroundColor;
            public Color outlineColor;
            public Color textColor;
            public Color rankBackgroundColor;
            public Color rankOutlineColor;
            public Color rankTextColor;
        }
    }
}
#endif

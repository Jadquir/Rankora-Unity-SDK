#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Rankora_API.Scripts.Rankora.Types;
using System.IO;

namespace Rankora_API.Editor
{
    /// <summary>
    /// Editor utility for generating default entry styles as ScriptableObject assets.
    /// </summary>
    public class RankoraEntryStyleGenerator : EditorWindow
    {
        [MenuItem("Rankora API/Generate Default Entry Styles", priority = 10)]
        public static void GenerateDefaultStyles()
        {
            string folderPath = "Assets/Rankora API/EntryStyles";
            
            // Create folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parent = "Assets/Rankora API";
                string leaf = "EntryStyles";
                AssetDatabase.CreateFolder(parent, leaf);
            }

            // Generate Gold Style (1st Place)
            var goldStyle = CreateInstance<EntryStyle>();
            goldStyle.name = "GoldEntryStyle";
            goldStyle.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            goldStyle.outlineColor = new Color(0.8f, 0.65f, 0.1f);
            goldStyle.outlineDistance = new Vector2(1f, -1f);
            goldStyle.textColor = new Color(1.0f, 0.85f, 0.3f);
            goldStyle.rankBackgroundColor = new Color(0.15f, 0.15f, 0.15f);
            goldStyle.rankOutlineColor = new Color(1.0f, 0.84f, 0.0f);
            goldStyle.rankOutlineDistance = new Vector2(1.5f, -1.5f);
            goldStyle.rankTextColor = new Color(1.0f, 0.84f, 0.0f);
            
            AssetDatabase.CreateAsset(goldStyle, Path.Combine(folderPath, "GoldEntryStyle.asset"));

            // Generate Silver Style (2nd Place)
            var silverStyle = CreateInstance<EntryStyle>();
            silverStyle.name = "SilverEntryStyle";
            silverStyle.backgroundColor = new Color(0.13f, 0.13f, 0.13f);
            silverStyle.outlineColor = new Color(0.7f, 0.7f, 0.8f);
            silverStyle.outlineDistance = new Vector2(1f, -1f);
            silverStyle.textColor = new Color(0.85f, 0.85f, 0.95f);
            silverStyle.rankBackgroundColor = new Color(0.13f, 0.13f, 0.13f);
            silverStyle.rankOutlineColor = new Color(0.75f, 0.75f, 0.75f);
            silverStyle.rankOutlineDistance = new Vector2(1.5f, -1.5f);
            silverStyle.rankTextColor = new Color(0.85f, 0.85f, 0.85f);
            
            AssetDatabase.CreateAsset(silverStyle, Path.Combine(folderPath, "SilverEntryStyle.asset"));

            // Generate Bronze Style (3rd Place)
            var bronzeStyle = CreateInstance<EntryStyle>();
            bronzeStyle.name = "BronzeEntryStyle";
            bronzeStyle.backgroundColor = new Color(0.11f, 0.11f, 0.11f);
            bronzeStyle.outlineColor = new Color(0.7f, 0.4f, 0.2f);
            bronzeStyle.outlineDistance = new Vector2(1f, -1f);
            bronzeStyle.textColor = new Color(0.9f, 0.6f, 0.3f);
            bronzeStyle.rankBackgroundColor = new Color(0.11f, 0.11f, 0.11f);
            bronzeStyle.rankOutlineColor = new Color(0.8f, 0.5f, 0.2f);
            bronzeStyle.rankOutlineDistance = new Vector2(1.5f, -1.5f);
            bronzeStyle.rankTextColor = new Color(0.9f, 0.6f, 0.3f);
            
            AssetDatabase.CreateAsset(bronzeStyle, Path.Combine(folderPath, "BronzeEntryStyle.asset"));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the generated folder
            var folderAsset = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
            Selection.activeObject = folderAsset;
            EditorGUIUtility.PingObject(folderAsset);

            Debug.Log("[Rankora] Generated default entry styles in: " + folderPath);
            EditorUtility.DisplayDialog("Entry Styles Generated", 
                "Default entry styles have been generated successfully!\n\n" +
                "• GoldEntryStyle.asset (1st Place)\n" +
                "• SilverEntryStyle.asset (2nd Place)\n" +
                "• BronzeEntryStyle.asset (3rd Place)\n\n" +
                "The styles folder has been selected in the Project window.", "OK");
        }

        [MenuItem("Rankora API/Open Entry Styles Folder", priority = 11)]
        public static void OpenEntryStylesFolder()
        {
            string folderPath = "Assets/Rankora API/EntryStyles";
            
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                var folderAsset = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
                Selection.activeObject = folderAsset;
                EditorGUIUtility.PingObject(folderAsset);
            }
            else
            {
                EditorUtility.DisplayDialog("Folder Not Found", 
                    "The EntryStyles folder doesn't exist yet. Use 'Generate Default Entry Styles' to create it first.", "OK");
            }
        }

        /// <summary>
        /// Returns a list of the generated default styles for auto-assignment.
        /// </summary>
        public static System.Collections.Generic.List<EntryStyle> GetGeneratedStyles()
        {
            var styles = new System.Collections.Generic.List<EntryStyle>();
            string folderPath = "Assets/Rankora API/EntryStyles";
            
            if (!AssetDatabase.IsValidFolder(folderPath))
                return styles;
            
            // Try to find the generated styles
            var goldStyle = AssetDatabase.LoadAssetAtPath<EntryStyle>(Path.Combine(folderPath, "GoldEntryStyle.asset"));
            var silverStyle = AssetDatabase.LoadAssetAtPath<EntryStyle>(Path.Combine(folderPath, "SilverEntryStyle.asset"));
            var bronzeStyle = AssetDatabase.LoadAssetAtPath<EntryStyle>(Path.Combine(folderPath, "BronzeEntryStyle.asset"));
            
            if (goldStyle != null) styles.Add(goldStyle);
            if (silverStyle != null) styles.Add(silverStyle);
            if (bronzeStyle != null) styles.Add(bronzeStyle);
            
            return styles;
        }

        /// <summary>
        /// Checks if the default styles already exist in the project.
        /// </summary>
        public static bool DefaultStylesExist()
        {
            string folderPath = "Assets/Rankora API/EntryStyles";
            
            if (!AssetDatabase.IsValidFolder(folderPath))
                return false;
            
            var goldStyle = AssetDatabase.LoadAssetAtPath<EntryStyle>(Path.Combine(folderPath, "GoldEntryStyle.asset"));
            var silverStyle = AssetDatabase.LoadAssetAtPath<EntryStyle>(Path.Combine(folderPath, "SilverEntryStyle.asset"));
            var bronzeStyle = AssetDatabase.LoadAssetAtPath<EntryStyle>(Path.Combine(folderPath, "BronzeEntryStyle.asset"));
            
            return goldStyle != null && silverStyle != null && bronzeStyle != null;
        }
    }
}
#endif

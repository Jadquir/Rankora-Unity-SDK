using UnityEngine;

namespace Rankora_API.Scripts.Rankora.Types
{
    /// <summary>
    /// ScriptableObject that stores style data for leaderboard entry UI elements.
    /// Can be created and customized in the Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "EntryStyle", menuName = "Rankora API/Entry Style")]
    public class EntryStyle : ScriptableObject
    {
        [Header("Entry Background")]
        [Tooltip("Background color for the entry")]
        public Color backgroundColor = Color.white;
        
        [Tooltip("Outline color for the entry")]
        public Color outlineColor = Color.white;
        
        [Tooltip("Outline distance/offset for the entry")]
        public Vector2 outlineDistance = Vector2.one;
        
        [Tooltip("Text color for the entry")]
        public Color textColor = Color.black;

        [Header("Rank Style")]
        [Tooltip("Background color for the rank number")]
        public Color rankBackgroundColor = Color.white;
        
        [Tooltip("Outline color for the rank number")]
        public Color rankOutlineColor = Color.white;
        
        [Tooltip("Outline distance/offset for the rank number")]
        public Vector2 rankOutlineDistance = Vector2.one;
        
        [Tooltip("Text color for the rank number")]
        public Color rankTextColor = Color.black;

        /// <summary>
        /// Creates a copy of the style data as a regular class instance.
        /// </summary>
        public EntryStyleData Clone()
        {
            return new EntryStyleData
            {
                backgroundColor = this.backgroundColor,
                outlineColor = this.outlineColor,
                outlineDistance = this.outlineDistance,
                textColor = this.textColor,
                rankBackgroundColor = this.rankBackgroundColor,
                rankOutlineColor = this.rankOutlineColor,
                rankOutlineDistance = this.rankOutlineDistance,
                rankTextColor = this.rankTextColor
            };
        }

        /// <summary>
        /// Resets all colors to default values.
        /// </summary>
        [ContextMenu("Reset to Default")]
        public void ResetToDefault()
        {
            backgroundColor = Color.white;
            outlineColor = Color.white;
            outlineDistance = Vector2.one;
            textColor = Color.black;
            rankBackgroundColor = Color.white;
            rankOutlineColor = Color.white;
            rankOutlineDistance = Vector2.one;
            rankTextColor = Color.black;
        }
    }

    /// <summary>
    /// Data structure for entry styles that can be serialized and used in runtime.
    /// </summary>
    [System.Serializable]
    public class EntryStyleData
    {
        public Color backgroundColor = Color.white;
        public Color outlineColor = Color.white;
        public Vector2 outlineDistance = Vector2.one;
        public Color textColor = Color.black;
        public Color rankBackgroundColor = Color.white;
        public Color rankOutlineColor = Color.white;
        public Vector2 rankOutlineDistance = Vector2.one;
        public Color rankTextColor = Color.black;
    }
}

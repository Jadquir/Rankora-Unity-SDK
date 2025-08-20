#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Assets.Rankora_API.Scripts.Rankora.Player;

namespace Rankora_API.Editor
{
    public class RankoraPlayerIdEditorWindow : EditorWindow
    {
        private string playerId;

        [MenuItem("Rankora API/Edit Player ID")]
        public static void ShowWindow()
        {
            var window = GetWindow<RankoraPlayerIdEditorWindow>("Edit Player ID");
            window.minSize = new Vector2(400, 100);
            window.LoadPlayerId();
            window.Show();
        }

        private void LoadPlayerId()
        {
            playerId = RankoraPlayerId.GetSavedPlayerId();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Player ID", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            playerId = EditorGUILayout.TextField("Value", playerId);

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                RankoraPlayerId.SetSavedPlayerId(playerId);
                Debug.Log($"[Rankora] Player ID updated: {playerId}");
                Repaint();
            }

            if (GUILayout.Button("Reload"))
            {
                LoadPlayerId();
                Debug.Log("[Rankora] Player ID reloaded from storage.");
                Repaint();
            }

            if (GUILayout.Button("Clear"))
            {
                if (EditorUtility.DisplayDialog("Clear Player ID", "Are you sure you want to clear the saved Player ID?", "Yes", "Cancel"))
                {
                    RankoraPlayerId.ClearSavedPlayerId();
                    playerId = string.Empty;
                    Debug.Log("[Rankora] Player ID cleared.");
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

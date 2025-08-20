#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Rankora_API.Editor
{
    public class RankoraWelcomeWindow : EditorWindow
    {
        private const string ShowOnStartupKey = "Rankora_WelcomeWindow_Shown";
        private static readonly string DashboardUrl = "https://rankora.dev/dashboard";
        private static Texture2D _logo;

        [InitializeOnLoadMethod]
        private static void ShowOnFirstLoad()
        {
            if (!EditorPrefs.GetBool(ShowOnStartupKey, false))
            {
                EditorApplication.delayCall += () =>
                {
                    ShowWindow();
                    EditorPrefs.SetBool(ShowOnStartupKey, true);
                };
            }
        }

        [MenuItem("Rankora API/Show Welcome Window", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<RankoraWelcomeWindow>(true, "Welcome to Rankora!");
            window.minSize = new Vector2(450, 300);
            window.maxSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            // Load logo from Resources (e.g., "Rankora API/RankoraLogo")
            _logo = Resources.Load<Texture2D>("Assets/Rankora API/Sprites/Rankora App Icon Transparent.png");
        }

        private void OnGUI()
        {
            // Background panel
            EditorGUILayout.BeginVertical("box");
            GUILayout.Space(10);

            // Logo
            if (_logo != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(_logo, GUILayout.Width(128), GUILayout.Height(128));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            // Title
            GUILayout.Label("Welcome to Rankora!", EditorStyles.boldLabel);
            GUILayout.Space(6);

            // Subtitle
            EditorGUILayout.LabelField(
                "Your all-in-one leaderboard and analytics solution for Unity.",
                EditorStyles.wordWrappedLabel
            );
            GUILayout.Space(12);

            // Info box
            EditorGUILayout.HelpBox(
                "To get started, open the Rankora dashboard and get your API key. Set it in the Rankora Settings.",
                MessageType.Info
            );
            GUILayout.Space(12);

            // Buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Rankora Dashboard", GUILayout.Height(40)))
            {
                Application.OpenURL(DashboardUrl);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Open Rankora Settings", GUILayout.Height(40)))
            {
                var settings = Resources.Load("Rankora API/RankoraSettings");
                if (settings != null)
                {
                    Selection.activeObject = settings;
                    EditorGUIUtility.PingObject(settings);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Rankora Settings Not Found",
                        "Could not find the RankoraSettings asset in Resources/Rankora API. Please ensure the SDK is imported correctly.",
                        "OK"
                    );
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            // Footer
            EditorGUILayout.LabelField("\u00A9 2025 Rankora", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(8);
            EditorGUILayout.EndVertical();
        }
    }
}
#endif

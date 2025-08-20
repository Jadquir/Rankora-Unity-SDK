using Rankora_API.Examples.Visual.Scripts;
using Rankora_API.Scripts.Rankora.Main;
using Rankora_API.Scripts.Rankora.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rankora_API.Scripts.UI
{
    /// <summary>
    /// Handles UI for submitting player name and score to the leaderboard.
    /// Supports debug input fields in the Unity Editor.
    /// </summary>
    [AddComponentMenu("Rankora/UI/Submit Score UI")]
    [Icon("Assets/Rankora API/Sprites/ScriptIcons/Send.png")]
    public class SubmitScoreUI : MonoBehaviour
    {
        public TMP_InputField PlayerNameInputField; // Input for player name
        public Button SubmitButton; // Button to submit the score
        public TMP_Text ErrorText;  // Displays error messages

        public UnityEvent OnSubmitScoreSucceded; // Event fired on successful submission

#if UNITY_EDITOR
        [Header("Debug Values (Only avaliable in Unity Editor)")]
        public bool EnableScoreFieldValue = false; // Enable manual score input in editor
        public TMP_InputField PlayerIdInputField; // Debug field to show player ID
        public TMP_InputField ScoreField;          // Debug field to input score manually
#endif

        private void Awake()
        {
            SetErrorText(""); // Clear any error messages initially

            // Subscribe to player update events to update UI accordingly
            RankoraEvents.OnPostPlayerUpdated.Subscribe(OnPlayerIdUpdated);
            RankoraEvents.OnPlayerFetched.Subscribe(OnPlayerFetched);

            // Fetch current player data
            RankoraPlayer.Instance.Get();
        }

        /// <summary>
        /// Called when player data is fetched. Updates UI fields.
        /// </summary>
        private void OnPlayerFetched(PlayerEntry entry)
        {
            PlayerNameInputField.text = entry.name;

#if UNITY_EDITOR
            PlayerIdInputField.text = entry.player_id;
            ScoreField.text = entry.score.ToString();
#endif
        }

        /// <summary>
        /// Called when player info is updated (like after score submission).
        /// Updates player ID debug field if successful.
        /// </summary>
        private void OnPlayerIdUpdated(PostPlayerResponse response)
        {
            if (!response.success) return;

#if UNITY_EDITOR
            PlayerIdInputField.text = response.player_id;
#endif
        }

        /// <summary>
        /// Sets the player score directly.
        /// </summary>
        public void SetScore(double score)
        {
            RankoraPlayer.Instance.Score = score;
        }

        /// <summary>
        /// Submit the given score along with the player name from input field.
        /// Disables submit button while loading, clears error messages.
        /// </summary>
        public void SubmitScore(double score)
        {
            SetButtonInteractable(SubmitButton, loading: true);
            SetErrorText("");

            RankoraPlayer.Instance.PlayerName = PlayerNameInputField.text.Trim();
            RankoraPlayer.Instance.Score = score;
            RankoraPlayer.Instance.Sync(OnPlayerUpdated);
        }

        /// <summary>
        /// Submits the score.
        /// In editor, can use manual score input if enabled, otherwise uses current player score.
        /// </summary>
        public void SubmitScore()
        {
#if UNITY_EDITOR
            if (EnableScoreFieldValue)
            {
                SubmitScore(double.Parse(ScoreField.text));
            }
            else
            {
                SubmitScore(RankoraPlayer.Instance.Score);
            }
#else
            SubmitScore(RankoraPlayer.Instance.Score);
#endif
        }

        /// <summary>
        /// Callback after syncing player data with the server.
        /// Enables submit button and displays success or error message.
        /// </summary>
        private void OnPlayerUpdated(PostPlayerResponse response)
        {
            SetButtonInteractable(SubmitButton, loading: false);

            if (response.success)
            {
                OnSubmitScoreSucceded.Invoke();
                Debug.Log("Player score submitted successfully.");
            }
            else
            {
                SetErrorText("Failed to submit player score: " + response.error);
            }
        }

        /// <summary>
        /// Sets button interactable state or loading state if using LoadableButton component.
        /// </summary>
        void SetButtonInteractable(Button button, bool loading)
        {
            if (button.TryGetComponent<LoadableButton>(out var loadable))
            {
                loadable.SetLoading(loading);
            }
            else
            {
                button.interactable = !loading;
            }
        }

        /// <summary>
        /// Shows or hides error text UI and sets its message.
        /// </summary>
        void SetErrorText(string text)
        {
            if (ErrorText == null) return;

            ErrorText.gameObject.SetActive(!string.IsNullOrEmpty(text));
            ErrorText.text = text;
        }
    }
}

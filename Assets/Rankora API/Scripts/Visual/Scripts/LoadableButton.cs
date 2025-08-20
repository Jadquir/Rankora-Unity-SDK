using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rankora_API.Examples.Visual.Scripts
{
    [RequireComponent(typeof(Button))]
    public class LoadableButton : MonoBehaviour
    {
        private Button _button;
        public Button Button => _button == null ? _button = GetComponent<Button>() : _button;

        [SerializeField] private string LoadingText;

        string defaultText = string.Empty;
        TMP_Text ButtonText;
        private void Awake()
        {
            ButtonText = Button.GetComponentInChildren<TMP_Text>();
            if (ButtonText == null)
            {
                Debug.LogError("LoadableButton cannot be used without text");
                return;
            }
            defaultText = ButtonText.text;
        }

        public void SetLoading(bool loading)
        {
            this.Button.interactable = !loading;

            var text = defaultText;
            if (loading && !string.IsNullOrEmpty(LoadingText)) {
                text = LoadingText;
            }
            ButtonText.text = text;
        }
    }
}


using Assets.Rankora_API.Scripts.Visual.Scripts;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace Rankora_API.Examples.Visual.Scripts
{
        [RequireComponent(typeof(CanvasGroup))]
        public class Overlay : MonoBehaviour
        {
            [Header("UI References")]
            [SerializeField] private Button closeButton;
            [SerializeField] private Button overlayBackground;

            [Header("Config")]
            [SerializeField] private bool canCloseOverlayWithClickToBackground = true;
            [SerializeField] private float fadeDuration = 0.25f;

            private CanvasGroup _canvasGroup;
            public CanvasGroup canvasGroup => _canvasGroup == null ? _canvasGroup = GetComponent<CanvasGroup>() : _canvasGroup;

            private bool canClose = true;
            private FadeObject fader;

            private void Awake()
            {
                if (closeButton != null)
                    closeButton.onClick.AddListener(RequestCloseClick);

                if (canCloseOverlayWithClickToBackground && overlayBackground != null)
                    overlayBackground.onClick.AddListener(RequestCloseClick);

                fader = FadeObject.CreateFade(transform, fadeDuration);
            }

            public void SetCloseable(bool canClose)
            {
                this.canClose = canClose;

                if (closeButton != null)
                    closeButton.interactable = canClose;
            }

            public void Open()
            {
                fader.FadeIn();
            }

            public void Close(bool forceClose = false)
            {
                RequestClose(forceClose);
            }

            private void RequestCloseClick()
            {
                RequestClose();
            }

            private void RequestClose(bool forceClose = false)
            {
                if (!canClose && !forceClose) return;
                fader.FadeOut();
                //if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                //fadeRoutine = StartCoroutine(FadeCanvas(0f, false));
            }

        }

}

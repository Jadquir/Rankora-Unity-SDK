using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Rankora_API.Scripts.Visual.Scripts
{
    public class FadeObject : MonoBehaviour
    {
        public static FadeObject CreateFade(Transform transform, float duration)
        {
            if(transform == null) { return null; }
            if(!transform.TryGetComponent<FadeObject>(out var fader))
            {
                fader = transform.gameObject.AddComponent<FadeObject>();
            }
            fader.SetDuration(duration);
            return fader;
        }

        private void SetDuration(float duration)
        {
            fadeDuration = duration;
        }

        [SerializeField] float fadeDuration = .2f;

        public UnityEvent OnFadeInComplete;
        public UnityEvent OnFadeOutComplete;

        CanvasGroup canvasGroup;
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if(canvasGroup == null)
                canvasGroup = transform.gameObject.AddComponent<CanvasGroup>();

            SetCanvas(0, false);
        }
        private Coroutine fadeRoutine;

        public void FadeIn()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            gameObject.SetActive(true);
            fadeRoutine = StartCoroutine(FadeCanvas(1f, true));
        }
        public void FadeOut()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeCanvas(0f, false));
        }
        public bool DisableAfterFadeout = true;
        private IEnumerator FadeCanvas(float targetAlpha, bool opening)
        {
            float startAlpha = canvasGroup.alpha;
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            SetCanvas(targetAlpha, opening);

            var unityEvent = opening ? OnFadeInComplete : OnFadeOutComplete;
            unityEvent?.Invoke();

            if (!opening && DisableAfterFadeout)
                gameObject.SetActive(false);

            fadeRoutine = null;
        }

        private void SetCanvas(float targetAlpha, bool opening)
        {
            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = opening;
            canvasGroup.blocksRaycasts = opening;
        }
    }
}

using System;
using System.Collections;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// フェード管理クラス
    /// </summary>
    public class FadeManager : Singleton<FadeManager>
    {
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        protected override bool UseDontDestroyOnLoad => true;

        private Coroutine fadeCoroutine;

        public override void Init()
        {
            base.Init();
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f; 
                fadeCanvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// フェードイン（暗転 → 表示）
        /// </summary>
        /// <param name="duration">フェード時間</param>
        /// <param name="onComplete">完了時のコールバック</param>
        public void FadeIn(float duration, Action onComplete = null)
        {
            StartFade(from: 1f, to: 0f, duration, false, onComplete);
        }

        /// <summary>
        /// フェードアウト（表示 → 暗転）
        /// </summary>
        /// <param name="duration">フェード時間</param>
        /// <param name="onComplete">完了時のコールバック</param>
        public void FadeOut(float duration, Action onComplete = null)
        {
            StartFade(from: 0f, to: 1f, duration, true, onComplete);
        }

        /// <summary>
        /// 即座に暗転状態にする
        /// </summary>
        public void SetBlack()
        {
            if (fadeCanvasGroup == null)
            {
                Debug.LogError("Fade CanvasGroup is not assigned.");
                return;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 即座に透明状態にする
        /// </summary>
        public void SetClear()
        {
            if (fadeCanvasGroup == null)
            {
                Debug.LogError("Fade CanvasGroup is not assigned.");
                return;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// フェード実行中かどうか
        /// </summary>
        public bool IsFading => fadeCoroutine != null;

        private void StartFade(float from, float to, float duration, bool blockRaycasts, Action onComplete)
        {
            if (fadeCanvasGroup == null)
            {
                Debug.LogError("Fade CanvasGroup is not assigned.");
                onComplete?.Invoke();
                return;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCanvasGroup.blocksRaycasts = blockRaycasts;
            fadeCoroutine = StartCoroutine(FadeCoroutine(from, to, duration, onComplete));
        }

        private IEnumerator FadeCoroutine(float from, float to, float duration, Action onComplete)
        {
            float time = 0f;
            fadeCanvasGroup.alpha = from;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
                yield return null;
            }

            fadeCanvasGroup.alpha = to;
            
            // フェードイン完了時はレイキャストをブロックしない
            if (to == 0f)
            {
                fadeCanvasGroup.blocksRaycasts = false;
            }

            fadeCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
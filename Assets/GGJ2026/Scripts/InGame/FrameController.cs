using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GGJ2026.InGame
{
    /// <summary>
    /// ゲームの枠をコントロールするクラス
    /// </summary>
    public class FrameController : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private float duration = 1.0f;

        private Material runtimeMaterial;
        private Coroutine progressCoroutine;

        private static readonly int ProgressId = Shader.PropertyToID("_Progress");

        private void Awake()
        {
            if (image == null)
            {
                Debug.LogError("Image is not assigned.");
                enabled = false;
                return;
            }

            // マテリアルを複製（超重要）
            runtimeMaterial = Instantiate(image.material);
            image.material = runtimeMaterial;
        }

        /// <summary>
        /// 下から上に表示（0 → 1）
        /// </summary>
        [ContextMenu("a")]
        public void Reveal()
        {
            StartProgress(0f, 1f);
        }

        /// <summary>
        /// 上から消える（1 → 0）
        /// </summary>
        public void Hide()
        {
            StartProgress(1f, 0f);
        }

        private void StartProgress(float from, float to)
        {
            if (progressCoroutine != null)
                StopCoroutine(progressCoroutine);

            progressCoroutine = StartCoroutine(ProgressRoutine(from, to));
        }

        private IEnumerator ProgressRoutine(float from, float to)
        {
            float time = 0f;
            runtimeMaterial.SetFloat(ProgressId, from);

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                float value = Mathf.Lerp(from, to, t);

                runtimeMaterial.SetFloat(ProgressId, value);
                yield return null;
            }

            runtimeMaterial.SetFloat(ProgressId, to);
            progressCoroutine = null;
        }
    }
}

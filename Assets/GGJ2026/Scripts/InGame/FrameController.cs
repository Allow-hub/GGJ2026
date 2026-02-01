using System.Collections;
using System.Collections.Generic;
using GGJ2026.Core.Managers;
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

        [SerializeField] private FrameImageSet[] frameImageSets;

        private Dictionary<MaskType, FrameImageSet> frameImageDict = new Dictionary<MaskType, FrameImageSet>();

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

            // Dictionaryの構築（FrameImageSetごと保存）
            foreach (var set in frameImageSets)
            {
                if (set.image != null)
                {
                    frameImageDict[set.maskType] = set;
                }
            }
        }

        private void Start()
        {
            if (InGameManager.IsValid())
            {
                InGameManager.I.EventBus.Subscribe<InGameEvent.ApplyMainMaskEvent>(ChangeMask);
            }
        }

        private void OnDestroy()
        {
            if (InGameManager.IsValid())
            {
                InGameManager.I.EventBus.Unsubscribe<InGameEvent.ApplyMainMaskEvent>(ChangeMask);
            }
        }

        private void ChangeMask(InGameEvent.ApplyMainMaskEvent e)
        {
            if (frameImageDict.TryGetValue(e.SelectedItem.Config.itemName, out var targetSet))
            {
                // アルファを0に設定
                runtimeMaterial.SetFloat(ProgressId, 0f);
                
                // スプライトとカラーを変更
                image.sprite = targetSet.image;
                image.color = targetSet.color;
                
                // 0 → 1 のプログレスアニメーションを開始
                StartProgress(0f, 1f);
            }
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

    [System.Serializable]
    public class FrameImageSet
    {
        public MaskType maskType;
        public Sprite image;
        public Color color = Color.white; // デフォルト値
    }
}
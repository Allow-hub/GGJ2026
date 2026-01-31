using System.Collections;
using System.Collections.Generic;
using GGJ2026.Core.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GGJ2026.Title
{
    /// <summary>
    /// タイトル画面を制御するクラス
    /// </summary>
    public class TitleController : MonoBehaviour
    {
        [SerializeField] private Button startButton;//スタートボタン
        private void Start()
        {
            startButton.onClick.AddListener(() => OnStartButtonClicked());
        }

        private void OnStartButtonClicked()
        {
            // ゲーム開始処理
            StartCoroutine(ChangeToInGameAfterFade());
        }


        private IEnumerator ChangeToInGameAfterFade()
        {
            bool fadeOutComplete = false;
            FadeManager.I.FadeOut(1.0f, () => fadeOutComplete = true);
            yield return new WaitUntil(() => fadeOutComplete);
            FadeManager.I.FadeIn(1.0f);
            GameManager.I.ChangeInGameState();
        }
    }
}

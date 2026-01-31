using System;
using System.Collections;
using System.Collections.Generic;
using GGJ2026.InGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// インゲームUIを管理するマネージャークラス
    /// </summary>
    public class UiManager : Singleton<UiManager>
    {
        [Header("強化メニュー関連")]
        [SerializeField] private CanvasGroup improveMenuCanvasGroup;//強化メニュー


        [Header("マスクグリッド関連")]
        [SerializeField] private CanvasGroup gridCanvasGroup;//マスクグリッド
        [SerializeField] private Button openImproveMenuButton;//強化メニューを開くボタン
        [SerializeField] private Button closeImproveMenuButton;//強化メニューを閉じ

        [Header("リワード関連")]
        [SerializeField] private CanvasGroup rewardCanvasGroup;//リワードUI
        [SerializeField] private TextMeshProUGUI[] rewardText = new TextMeshProUGUI[3];//リワードテキスト
        [SerializeField] private Image[] rewardImage = new Image[3];//リワード画像

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            ShowGrid();
            openImproveMenuButton.onClick.AddListener(() => ShowImproveMenu());
            closeImproveMenuButton.onClick.AddListener(() => ShowGrid());
            Set(rewardCanvasGroup, false);
            InGameManager.I.EventBus.Subscribe<InGameEvent.OnRewardStartEvent>(e => ShowReward());
        }

        private void ShowImproveMenu()
        {
            Set(improveMenuCanvasGroup, true);
            Set(gridCanvasGroup, false);
        }

        private void ShowGrid()
        {
            Set(improveMenuCanvasGroup, false);
            Set(gridCanvasGroup, true);
        }

        private void ShowReward()
        {
            Set(rewardCanvasGroup, true);
            //マスクの内容を受け取れるようになったらリワード内容の設定をしてください
        }

        /// <summary>
        /// CanvasGroupの表示状態を設定する
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="active"></param>
        private void Set(CanvasGroup cg, bool active)
        {
            cg.alpha = active ? 1f : 0f;
            cg.interactable = active;
            cg.blocksRaycasts = active;
        }
    }
}

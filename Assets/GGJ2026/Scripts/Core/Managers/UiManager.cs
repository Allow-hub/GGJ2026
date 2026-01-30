using System;
using System.Collections;
using System.Collections.Generic;
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
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            ShowGrid();
            openImproveMenuButton.onClick.AddListener(() => ShowImproveMenu());
            closeImproveMenuButton.onClick.AddListener(() => ShowGrid());
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

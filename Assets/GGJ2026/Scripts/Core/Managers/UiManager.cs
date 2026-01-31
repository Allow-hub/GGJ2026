using System;
using GGJ2026.Events;
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
        [SerializeField] private Button hpImproveButton;//HP強化ボタン
        [SerializeField] private TextMeshProUGUI hpImproveLevelText;//HP強化ボレベルテキスト
        [SerializeField] private Button attackImproveButton;//攻撃力強化ボタン
        [SerializeField] private TextMeshProUGUI attackImproveLevelText;//攻撃力強化レベルテキスト
        [SerializeField] private Button speedImproveButton;//速度強化ボタン
        [SerializeField] private TextMeshProUGUI speedImproveLevelText;//速度強化レベルテキスト
        [SerializeField] private Button healButton;//回復ボタン
        [SerializeField] private TextMeshProUGUI healLevelText;//回復レベルテキスト

        [Header("マスクグリッド関連")]
        [SerializeField] private CanvasGroup gridCanvasGroup;//マスクグリッド
        [SerializeField] private Button openImproveMenuButton;//強化メニューを開くボタン
        [SerializeField] private Button closeImproveMenuButton;//強化メニューを閉じ

        [Header("リワード関連")]
        [SerializeField] private CanvasGroup rewardCanvasGroup;//リワードUI
        [SerializeField] private TextMeshProUGUI[] rewardText = new TextMeshProUGUI[3];//リワードテキスト
        [SerializeField] private Image[] rewardImage = new Image[3];//リワード画像

        [Header("レベルアップ設定")]
        [SerializeField] private int basePointCost = 10;//基本コスト
        [SerializeField] private float costMultiplier = 1.5f;//レベルごとのコスト倍率

        [Header("HP回復設定")]
        [SerializeField] private int healBasePointCost = 50;   // 回復は高め
        [SerializeField] private float healCostMultiplier = 2.0f;

        [SerializeField] private TextMeshProUGUI floorText;//現在のフロアテキスト

        private int lastFloor = 0;

        // 各強化のレベル
        private int hpLevel = 0;
        private int attackLevel = 0;
        private int speedLevel = 0;
        private int healLevel = 0;

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            ShowGrid();
            openImproveMenuButton.onClick.AddListener(() => ShowImproveMenu());
            closeImproveMenuButton.onClick.AddListener(() => ShowGrid());

            // 強化ボタンのリスナー設定
            hpImproveButton.onClick.AddListener(() => TryImprove(ref hpLevel, hpImproveLevelText, OnHpImproved));
            attackImproveButton.onClick.AddListener(() => TryImprove(ref attackLevel, attackImproveLevelText, OnAttackImproved));
            speedImproveButton.onClick.AddListener(() => TryImprove(ref speedLevel, speedImproveLevelText, OnSpeedImproved));
            healButton.onClick.AddListener(() => TryHealImprove());

            Set(rewardCanvasGroup, false);
            InGameManager.I.EventBus.Subscribe<InGameEvent.OnRewardStartEvent>(e => ShowReward());

            // 初期テキスト更新
            UpdateLevelText();
        }

        private void Update()
        {
            // ボタンの有効/無効を更新
            UpdateButtonStates();

            if (InGameManager.I.CurrentFloor != lastFloor)
            {
                lastFloor = InGameManager.I.CurrentFloor;
                floorText.text = $"{lastFloor}";
            }
        }

        /// <summary>
        /// レベルアップに必要なポイントを計算
        /// </summary>
        private int GetRequiredPoints(int currentLevel)
        {
            return Mathf.RoundToInt(basePointCost * Mathf.Pow(costMultiplier, currentLevel));
        }

        /// <summary>
        /// HP回復に必要なポイントを計算
        /// </summary>
        private int GetHealRequiredPoints(int currentLevel)
        {
            return Mathf.RoundToInt(healBasePointCost * Mathf.Pow(healCostMultiplier, currentLevel));
        }

        /// <summary>
        /// 強化を試みる
        /// </summary>
        private void TryImprove(ref int level, TextMeshProUGUI levelText, Action onImproved)
        {
            int requiredPoints = GetRequiredPoints(level);

            if (PointManager.I.Points >= requiredPoints)
            {
                PointManager.I.UsePoints(requiredPoints);
                level++;
                UpdateLevelText(levelText, level, false);
                onImproved?.Invoke();
            }
        }

        /// <summary>
        /// HP回復を試みる
        /// </summary>
        private void TryHealImprove()
        {
            int requiredPoints = GetHealRequiredPoints(healLevel);

            if (PointManager.I.Points >= requiredPoints)
            {
                PointManager.I.UsePoints(requiredPoints);
                healLevel++;
                UpdateLevelText(healLevelText, healLevel, true);
                OnHealImproved();
            }
        }

        /// <summary>
        /// すべてのレベルテキストを更新
        /// </summary>
        private void UpdateLevelText()
        {
            UpdateLevelText(hpImproveLevelText, hpLevel, false);
            UpdateLevelText(attackImproveLevelText, attackLevel, false);
            UpdateLevelText(speedImproveLevelText, speedLevel, false);
            UpdateLevelText(healLevelText, healLevel, true);
        }

        /// <summary>
        /// 特定のレベルテキストを更新
        /// </summary>
        private void UpdateLevelText(TextMeshProUGUI text, int level, bool isHeal)
        {
            int requiredPoints = isHeal ? GetHealRequiredPoints(level) : GetRequiredPoints(level);
            text.text = $"Lv.{level} (Cost: {requiredPoints})";
        }

        /// <summary>
        /// ボタンの有効/無効状態を更新
        /// </summary>
        private void UpdateButtonStates()
        {
            hpImproveButton.interactable = PointManager.I.Points >= GetRequiredPoints(hpLevel);
            attackImproveButton.interactable = PointManager.I.Points >= GetRequiredPoints(attackLevel);
            speedImproveButton.interactable = PointManager.I.Points >= GetRequiredPoints(speedLevel);
            healButton.interactable = PointManager.I.Points >= GetHealRequiredPoints(healLevel);
        }

        // 各強化が実行されたときの処理
        private void OnHpImproved()
        {
            InGameManager.I.EventBus.Publish(new ImproveEvents(PlayerParam.Health, hpLevel));
        }

        private void OnAttackImproved()
        {
            InGameManager.I.EventBus.Publish(new ImproveEvents(PlayerParam.AttackPower, attackLevel));
        }

        private void OnSpeedImproved()
        {
            InGameManager.I.EventBus.Publish(new ImproveEvents(PlayerParam.Agility, speedLevel));
        }

        private void OnHealImproved()
        {
            Debug.Log("Heal Improved");
            // InGameManager.I.EventBus.Publish(new ImproveEvents(PlayerParam.Heal, healLevel));
        }

        private void ShowImproveMenu()
        {
            Set(improveMenuCanvasGroup, true);
            Set(gridCanvasGroup, false);
            UpdateLevelText();
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
        private void Set(CanvasGroup cg, bool active)
        {
            cg.alpha = active ? 1f : 0f;
            cg.interactable = active;
            cg.blocksRaycasts = active;
        }
    }
}
using System;
using GGJ2026.Core.Audio;
using GGJ2026.Events;
using GGJ2026.InGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GGJ2026.InGame.InGameEvent;

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
        [SerializeField] private Button[] rewardButtons = new Button[3];//リワード選択ボタン

        [Header("レベルアップ設定")]
        [SerializeField] private int basePointCost = 10;//基本コスト
        [SerializeField] private float costMultiplier = 1.5f;//レベルごとのコスト倍率

        [Header("HP回復設定")]
        [SerializeField] private int healBasePointCost = 50;   // 回復は高め
        [SerializeField] private float healCostMultiplier = 2.0f;

        [Header("タイマー関連")]
        [SerializeField] private Slider timerSlider;//タイマースライダー
        [SerializeField] private Image timerFillImage;//スライダーの塗りつぶし画像（オプション）
        [SerializeField] private Color timerStartColor = Color.green;//開始時の色
        [SerializeField] private Color timerEndColor = Color.red;//終了時の色

        [SerializeField] private CanvasGroup maskDescriptionPopupCanvasGroup;//マスク説明ポップアップ
        [SerializeField] private TextMeshProUGUI maskDescriptionText;//マスク説明テキスト
        [SerializeField] private Button maskDescriptionCloseButton;//マスク説明ポップアップ閉じるボタン
        [SerializeField] private Button mainMaskApplyButton;//メインマスクに適用
        [SerializeField] private Button sellButton;//売却
        private ItemInstance currentoOpenMaskItem;
        private GameObject currentoOpenMaskObject;
        [SerializeField] private TextMeshProUGUI floorText;//現在のフロアテキスト
        [SerializeField] private TextMeshProUGUI hpText, atkText, spdText, ptText;

        private int lastFloor = 0;

        // 各強化のレベル
        private int hpLevel = 0;
        private int attackLevel = 0;
        private int speedLevel = 0;
        private int healLevel = 0;

        private int lastHp = 0;
        private int lastAtk = 0;
        private float lastSpd = 0;
        private int lastPt = 0;
        // リワードアイテムの保持
        private ItemInstance[] currentRewardItems = new ItemInstance[3];

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            ShowGrid();

            // リスナーを削除してから追加（重複防止）
            openImproveMenuButton.onClick.RemoveAllListeners();
            openImproveMenuButton.onClick.AddListener(() => ShowImproveMenu());

            closeImproveMenuButton.onClick.RemoveAllListeners();
            closeImproveMenuButton.onClick.AddListener(() => ShowGrid());

            // 強化ボタンのリスナー設定
            hpImproveButton.onClick.RemoveAllListeners();
            hpImproveButton.onClick.AddListener(() => TryImprove(ref hpLevel, hpImproveLevelText, OnHpImproved));

            attackImproveButton.onClick.RemoveAllListeners();
            attackImproveButton.onClick.AddListener(() => TryImprove(ref attackLevel, attackImproveLevelText, OnAttackImproved));

            speedImproveButton.onClick.RemoveAllListeners();
            speedImproveButton.onClick.AddListener(() => TryImprove(ref speedLevel, speedImproveLevelText, OnSpeedImproved));

            healButton.onClick.RemoveAllListeners();
            healButton.onClick.AddListener(() => TryHealImprove());

            maskDescriptionCloseButton.onClick.RemoveAllListeners();
            maskDescriptionCloseButton.onClick.AddListener(() => OnMaskDescriptionClose());

            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => OnSellMask());
            // メインマスク適用ボタンのリスナー設定
            mainMaskApplyButton.onClick.RemoveAllListeners();
            mainMaskApplyButton.onClick.AddListener(() => OnMainMaskApply());


            // リワードボタンのリスナー設定
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                int index = i; // クロージャ対策
                rewardButtons[i].onClick.RemoveAllListeners();
                rewardButtons[i].onClick.AddListener(() => OnRewardSelected(index));
            }

            // タイマースライダーの初期化
            InitializeTimerSlider();

            Set(rewardCanvasGroup, false);
            InGameManager.I.EventBus.Subscribe<OnRewardStartEvent>(e => ShowReward(e));
            ptText.text = lastPt.ToString();
            // 初期テキスト更新
            UpdateLevelText();
        }

        private void OnDestroy()
        {
            if (!InGameManager.IsValid()) return;
            InGameManager.I.EventBus.Unsubscribe<OnRewardStartEvent>(e => ShowReward(e));
        }

        private void Update()
        {
            // ボタンの有効/無効を更新
            UpdateButtonStates();
            UpdateStatusTexts();
            UpdateTimerSlider();

            if (PointManager.I.Points != lastPt)
            {
                lastPt = PointManager.I.Points;
                ptText.text = lastPt.ToString();
            }
            if (InGameManager.I.CurrentFloor != lastFloor)
            {
                lastFloor = InGameManager.I.CurrentFloor;
                floorText.text = $"{lastFloor}";
            }
        }

        /// <summary>
        /// タイマースライダーの初期化
        /// </summary>
        private void InitializeTimerSlider()
        {
            if (timerSlider != null)
            {
                timerSlider.minValue = 0f;
                timerSlider.maxValue = 1f;
                timerSlider.value = 0f; // 初期値を0に（時間が経つと増える）
                timerSlider.interactable = false; // スライダーは操作不可

                // Fill Imageがある場合は色を設定
                if (timerFillImage == null && timerSlider.fillRect != null)
                {
                    timerFillImage = timerSlider.fillRect.GetComponent<Image>();
                }

                if (timerFillImage != null)
                {
                    timerFillImage.color = timerStartColor;
                }
            }
        }

        /// <summary>
        /// タイマースライダーを更新
        /// </summary>
        private void UpdateTimerSlider()
        {
            if (timerSlider == null) return;

            float currentTime = InGameManager.I.CurrentTime;
            float gameDuration = InGameManager.I.GameDuration;

            // 経過時間の割合を計算（0.0 = 開始, 1.0 = 終了）
            float timeRatio = Mathf.Clamp01(currentTime / gameDuration);

            // スライダーの値を更新（時間とともに増加）
            timerSlider.value = timeRatio;

            // 色のグラデーションを更新（緑→赤へ）
            if (timerFillImage != null)
                timerFillImage.color = Color.Lerp(timerStartColor, timerEndColor, timeRatio);
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

        private void UpdateStatusTexts()
        {
            int hp = InGameManager.I.PlayerController.CurrentHp;
            int atk = InGameManager.I.PlayerController.AttackPower;
            float spd = InGameManager.I.PlayerController.Speed;

            if (hp != lastHp)
            {
                lastHp = hp;
                hpText.text = $"{hp}";
            }

            if (atk != lastAtk)
            {
                lastAtk = atk;
                atkText.text = $"{atk}";
            }

            if (spd != lastSpd)
            {
                lastSpd = spd;
                spdText.text = spd.ToString("F1");
            }
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
            AudioManager.I.PlaySE(SEID.ButtonClick);
            InGameManager.I.EventBus.Publish(new ImproveEvents(PlayerParam.Health, hpLevel));
        }

        private void OnAttackImproved()
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            InGameManager.I.EventBus.Publish(new ImproveEvents(PlayerParam.AttackPower, attackLevel));
        }

        private void OnSpeedImproved()
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            InGameManager.I.EventBus.Publish(new ImproveEvents(PlayerParam.Agility, speedLevel));
        }

        private void OnHealImproved()
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            InGameManager.I.EventBus.Publish(new ImproveEvents(PlayerParam.Recovery, healLevel));
        }

        /// <summary>
        /// リワードが選択されたときの処理
        /// </summary>
        private void OnRewardSelected(int index)
        {
            if (currentRewardItems[index] != null)
            {
                // 選択されたアイテムをPublish
                InGameManager.I.EventBus.Publish(new OnRewardSelectedEvent(currentRewardItems[index]));
                // リワードUIを非表示
                Set(rewardCanvasGroup, false);
                AudioManager.I.PlaySE(SEID.ButtonClick);
                // InGameManager.I.OnRewardFinish();
            }
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

        private void ShowReward(OnRewardStartEvent onRewardStartEvent)
        {
            // リワードアイテムを保存
            currentRewardItems[0] = onRewardStartEvent.Item1;
            currentRewardItems[1] = onRewardStartEvent.Item2;
            currentRewardItems[2] = onRewardStartEvent.Item3;

            SetRewardView(0, onRewardStartEvent.Item1);
            SetRewardView(1, onRewardStartEvent.Item2);
            SetRewardView(2, onRewardStartEvent.Item3);

            Set(rewardCanvasGroup, true);
        }

        private void SetRewardView(int index, ItemInstance rewardItem)
        {
            rewardText[index].text =
                $"<color=#ff4500>ActiveSkill\n</color>" +
                $"{rewardItem.Config.activeSkill.description}\n" +
                $"\n" +
                $"<color=#7cfc00>PassiveSkill\n</color>" +
                $"{rewardItem.PassiveSkill.GetDescription()}\n";

            rewardImage[index].sprite = rewardItem.Config.itemSprite;
        }

        /// <summary>
        /// マスクの説明を表示/非表示する
        /// </summary>
        /// <param name="open"></param>
        /// <param name="item"></param>
        public void OpenMaskDescriptionPopup(bool open, ItemInstance item = null, GameObject obj = null)
        {
            if (open)
            {
                maskDescriptionText.text =
                $"ActiveSkill {item.Config.activeSkill.skillName}\n" +
                $"{item.Config.activeSkill.description}" +
                $"PassiveSkill {item.PassiveSkill.Config._skillName}\n" +
                $"{item.PassiveSkill.GetDescription()}\n";
                currentoOpenMaskItem = item;
                currentoOpenMaskObject = obj;
            }
            else
            {
                maskDescriptionText.text = "";
                currentoOpenMaskItem = null;
                currentoOpenMaskObject = null;
            }
            Set(maskDescriptionPopupCanvasGroup, open);
        }

        private void OnMaskDescriptionClose()
        {
            OpenMaskDescriptionPopup(false);
        }

        private void OnMainMaskApply()
        {
            if (currentoOpenMaskItem != null)
                InGameManager.I.EventBus.Publish(new ApplyMainMaskEvent(currentoOpenMaskItem, currentoOpenMaskObject));
            else
                Debug.LogWarning("[UiManager] currentoOpenMaskItem is null!");

            OpenMaskDescriptionPopup(false);
        }


        private void OnSellMask()
        {
            if (currentoOpenMaskItem == null) return;
            PointManager.I.AddPoints(currentoOpenMaskItem.Config.price);
            
            //Sellイベントの送信
            InGameManager.I.EventBus.Publish(new SellMaskEvent(currentoOpenMaskItem, currentoOpenMaskObject));
            
            OnMaskDescriptionClose();
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
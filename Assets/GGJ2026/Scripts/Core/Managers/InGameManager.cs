using System.Collections;
using System.Collections.Generic;
using GGJ2026.InGame;
using GGJ2026.InGame.Enemy;
using TechC.VBattle.Core;
using TMPro;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// ゲーム中の管理を行う上位マネージャークラス
    /// </summary>
    public class InGameManager : Singleton<InGameManager>
    {
        [SerializeField] private float gameDuration = 180f;
        [SerializeField] private TextMeshProUGUI countdownText; //カウントダウンテキスト

        private EventBus eventBus;
        public EventBus EventBus
        {
            get
            {
                if (eventBus == null)
                    eventBus = new EventBus();
                return eventBus;
            }
        }
        public InGameState CurrentState { get; private set; }

        [SerializeField, ReadOnly] private float currentTime;
        public float CurrentTime => currentTime;

        private float aliveTimer = 0;
        [SerializeField, ReadOnly] private int currentFloor;
        public int CurrentFloor => currentFloor;

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            eventBus = new EventBus();

            currentTime = gameDuration;
            currentFloor = 1;
            aliveTimer =0;
            ChangeState(InGameState.Start);
        }

        private void Update()
        {
            if (CurrentState == InGameState.Result) return;

            currentTime -= Time.deltaTime;
            aliveTimer += Time.deltaTime;

            if (currentTime <= 0f)
            {
                currentTime = 0f;
                EndGame();
            }
        }

        public void ChangeState(InGameState next)
        {
            CurrentState = next;

            switch (next)
            {
                case InGameState.Start:
                    StartCoroutine(OnStart());
                    break;

                case InGameState.FloorStart:
                    OnFloorStart();
                    break;

                case InGameState.Battle:
                    OnBattleStart();
                    break;

                case InGameState.Reward:
                    OnRewardStart();
                    break;
            }
        }

        private IEnumerator OnStart()
        {
            countdownText.text = "3";
            yield return new WaitForSeconds(1f);
            countdownText.text = "2";
            yield return new WaitForSeconds(1f);
            countdownText.text = "1";
            yield return new WaitForSeconds(1f);
            countdownText.text = "Start!";
            yield return new WaitForSeconds(1f);
            ChangeState(InGameState.FloorStart);
            yield return new WaitForSeconds(1f);
            countdownText.text = "";
        }

        private void OnFloorStart()
        {
            // 敵スポーン
            EnemyFactory.I.CreateEnemy(currentFloor);
            // フロア演出
            ChangeState(InGameState.Battle);
        }

        private void OnBattleStart()
        {
        }

        private void OnRewardStart()
        {
            var item_1 = ItemFactory.I.ChooseItem();
            var item_2 = ItemFactory.I.ChooseItem();
            var item_3 = ItemFactory.I.ChooseItem();

            // 報酬UI表示 + 10秒カウント
            eventBus.Publish(new InGameEvent.OnRewardStartEvent(item_1, item_2, item_3));
        }

        [ContextMenu("End")]
        private void EndGame()
        {
            GameManager.I.SetAliveTimer(aliveTimer);
            GameManager.I.SetResultFloor(currentFloor);
            ChangeState(InGameState.Result);
            //失敗の演出の数秒後リザルト画面を表示
            GameManager.I.ChangeResultState();
        }

        public void OnBattleClear()
        {
            ChangeState(InGameState.Reward);
        }

        public void OnRewardFinish()
        {
            currentFloor++;
            ChangeState(InGameState.FloorStart);
        }
    }


    public enum InGameState
    {
        Start,
        FloorStart,
        Battle,
        Reward,
        Transition,
        Result
    }
}
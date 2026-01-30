using System.Collections;
using System.Collections.Generic;
using GGJ2026.InGame;
using TechC.VBattle.Core;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// ゲーム中の管理を行う上位マネージャークラス
    /// </summary>
    public class InGameManager : Singleton<InGameManager>
    {
        [SerializeField] private float gameDuration = 180f;

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
        [SerializeField, ReadOnly] private int currentFloor;
        public int CurrentFloor => currentFloor;

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            eventBus = new EventBus();

            currentTime = gameDuration;
            currentFloor = 1;

            ChangeState(InGameState.Start);
        }

        private void Update()
        {
            if (CurrentState == InGameState.Result) return;

            currentTime -= Time.deltaTime;

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
                    OnStart();
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

        private void OnStart()
        {
            ChangeState(InGameState.FloorStart);
        }

        private void OnFloorStart()
        {
            // フロア演出
            ChangeState(InGameState.Battle);
        }

        private void OnBattleStart()
        {
            // 敵スポーン通知
        }

        private void OnRewardStart()
        {
            eventBus.Publish(new InGameEvent.OnRewardStartEvent());
            // 報酬UI表示 + 10秒カウント
        }

        private void EndGame()
        {
            ChangeState(InGameState.Result);
            //失敗の演出の数秒後リザルト画面を表示
            // GameManager.I.ChangeResultState();
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
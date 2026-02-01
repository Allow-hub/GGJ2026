using GGJ2026.Core.Audio;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// ゲーム全体を管理する最上位マネージャー
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("State Prefabs")]
        [SerializeField] private GameObject titlePrefab;
        [SerializeField] private GameObject inGamePrefab;
        [SerializeField] private GameObject resultPrefab;

        [SerializeField] private GameState initialState = GameState.Title;
        private GameObject currentRoot;
        private GameState currentState = GameState.Title;

        private int resultFloor = 0;
        public int ResultFloor => resultFloor;
        private float aliveTime = 0;
        public float AliveTimer => aliveTime;

        protected override bool UseDontDestroyOnLoad => true;

        public override void Init()
        {
            base.Init();
        }

        private void Start()
        {
            ChangeState(initialState);
        }

        private void ChangeState(GameState newState)
        {
            ExitState(currentState);

            currentState = newState;

            EnterState(currentState);
        }

        private void EnterState(GameState state)
        {
            switch (state)
            {
                case GameState.Title:
                    AudioManager.I.PlayBGM(BGMID.Title);
                    currentRoot = Instantiate(titlePrefab);
                    SetAliveTimer(0);
                    SetResultFloor(0);
                    if (ScoreManager.I == null) break;
                    ScoreManager.I.ResetScore();//スコアをリセット
                    if (PointManager.I == null) break;
                    PointManager.I.ResetPoints();//ポイントもリセット
                    break;

                case GameState.InGame:
                    AudioManager.I.PlayBGM(BGMID.Battle);
                    SetAliveTimer(0);
                    SetResultFloor(0);
                    currentRoot = Instantiate(inGamePrefab);
                    break;

                case GameState.Result:
                    currentRoot = Instantiate(resultPrefab);
                    break;
            }
        }

        /// <summary>
        /// ステートから抜ける処理
        /// </summary>
        /// <param name="state">現在は特に使用していない</param>
        private void ExitState(GameState state)
        {
            if (currentRoot != null)
            {
                Destroy(currentRoot);
                currentRoot = null;
            }
        }

        public void SetAliveTimer(float t) => aliveTime = t;
        public void SetResultFloor(int f) => resultFloor = f;


        [ContextMenu("ChangeTitleState")]
        public void ChangeTitleState()  => ChangeState(GameState.Title);
        
        [ContextMenu("ChangeInGameState")]
        public void ChangeInGameState() => ChangeState(GameState.InGame);
        
        [ContextMenu("ChangeResultState")]
        public void ChangeResultState() => ChangeState(GameState.Result);
    }

    public enum GameState
    {
        Title,
        InGame,
        Result
    }
}
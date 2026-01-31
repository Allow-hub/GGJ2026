using System.Collections;
using System.Collections.Generic;
using GGJ2026.Core.Managers;
using GGJ2026.InGame.Enemy;
using UnityEngine;

namespace GGJ2026.InGame
{
    /// <summary>
    /// ゲーム中の初期化を行うクラス
    /// </summary>
    public class InGameStartup : MonoBehaviour
    {
        [SerializeField] private InGameManager inGameManager;
        [SerializeField] private UiManager uiManager;
        [SerializeField] private EnemyFactory enemyFactory;
        private void Awake()
        {
            inGameManager.Init();
            uiManager.Init();
            enemyFactory.Init();
        }
    }
}
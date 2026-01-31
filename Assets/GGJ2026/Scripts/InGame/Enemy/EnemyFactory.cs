using System.Collections;
using System.Collections.Generic;
using GGJ2026.Core.Managers;
using UnityEngine;

namespace GGJ2026.InGame.Enemy
{
    /// <summary>
    /// 敵生成ファクトリー
    /// 階層に応じて敵を生成する
    /// </summary>
    public class EnemyFactory : Singleton<EnemyFactory>
    {
        [Header("敵プレハブ")]
        [SerializeField] private GameObject enemyPrefab;

        [Header("基本ステータス")]
        [SerializeField] private int baseHP = 100;
        [SerializeField] private int baseATK = 10;
        [SerializeField] private int baseAGL = 5;

        [SerializeField] private float multiplierPerFloor = 0.02f;

        [Header("生成設定")]
        [SerializeField] private Transform spawnPoint;

        /// <summary>
        /// 現存している敵のリスト
        /// </summary>
        private List<EnemyController> currentEnemies = new List<EnemyController>();

        /// <summary>
        /// 現存している敵の数
        /// </summary>
        public int CurrentEnemyCount => currentEnemies.Count;

        /// <summary>
        /// 現存している敵のリスト（読み取り専用）
        /// </summary>
        public IReadOnlyList<EnemyController> CurrentEnemies => currentEnemies.AsReadOnly();

        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
            currentEnemies.Clear();
        }

        /// <summary>
        /// 敵を生成
        /// </summary>
        public EnemyController CreateEnemy(int floor)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is not assigned!");
                return null;
            }

            floor = Mathf.Max(floor, 1);

            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;

            GameObject enemyObj = Instantiate(enemyPrefab, position, Quaternion.identity);
            enemyObj.transform.SetParent(spawnPoint);

            EnemyController enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy == null)
            {
                Debug.LogError("EnemyController component not found on prefab!");
                Destroy(enemyObj);
                return null;
            }

            int hp = CalculateStat(baseHP, floor);
            int atk = CalculateStat(baseATK, floor);
            int agl = CalculateStat(baseAGL, floor);

            enemy.Init(hp, atk, agl, floor);

            AddEnemy(enemy);

            return enemy;
        }

        /// <summary>
        /// 指定位置に敵を生成
        /// </summary>
        public EnemyController CreateEnemy(int floor, Vector3 position)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is not assigned!");
                return null;
            }

            floor = Mathf.Max(floor, 1);

            GameObject enemyObj = Instantiate(enemyPrefab, position, Quaternion.identity);
            enemyObj.transform.SetParent(spawnPoint);

            EnemyController enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy == null)
            {
                Debug.LogError("EnemyController component not found on prefab!");
                Destroy(enemyObj);
                return null;
            }

            int hp = CalculateStat(baseHP, floor);
            int atk = CalculateStat(baseATK, floor);
            int agl = CalculateStat(baseAGL, floor);

            enemy.Init(hp, atk, agl, floor);

            AddEnemy(enemy);

            return enemy;
        }

        /// <summary>
        /// 敵をリストに追加
        /// </summary>
        private void AddEnemy(EnemyController enemy)
        {
            if (!currentEnemies.Contains(enemy))
                currentEnemies.Add(enemy);
        }

        /// <summary>
        /// 敵数を減らす（指定した敵を削除）
        /// </summary>
        public void DecrementEnemyCount(EnemyController enemy)
        {
            if (currentEnemies.Remove(enemy))
            {
                if (CurrentEnemyCount == 0)
                    InGameManager.I.ChangeState(InGameState.Reward);
            }
        }

        /// <summary>
        /// すべての敵をクリア
        /// </summary>
        public void ClearAllEnemies()
        {
            currentEnemies.Clear();
        }

        /// <summary>
        /// 階層に応じたステータス計算
        /// </summary>
        private int CalculateStat(int baseValue, int floor)
        {
            float multiplier = 1.0f + (floor - 1) * multiplierPerFloor;
            return Mathf.RoundToInt(baseValue * multiplier);
        }
    }
}
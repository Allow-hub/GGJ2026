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
        
        [Header("HP上昇率設定")]
        [SerializeField, Tooltip("1フロアごとの上昇率 (例: 0.05 = 5%)")] 
        private float hpGrowthPerFloor = 0.05f;
        [SerializeField, Tooltip("10フロアごとのボーナス上昇率 (例: 0.5 = 50%)")] 
        private float hpGrowthPer10Floors = 0.5f;

        [Header("ATK上昇率設定")]
        [SerializeField, Tooltip("1フロアごとの上昇率")] 
        private float atkGrowthPerFloor = 0.05f;
        [SerializeField, Tooltip("10フロアごとのボーナス上昇率")] 
        private float atkGrowthPer10Floors = 0.3f;

        [Header("AGL上昇率設定")]
        [SerializeField, Tooltip("1フロアごとの上昇率")] 
        private float aglGrowthPerFloor = 0.02f;
        [SerializeField, Tooltip("10フロアごとのボーナス上昇率")] 
        private float aglGrowthPer10Floors = 0.1f;

        [SerializeField] private float multiplierPerFloor = 0.02f;

        [Header("生成設定")]
        [SerializeField] private Transform spawnPoint;
        private enum StatType
        {
            HP,
            ATK,
            AGL
        }

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

            int hp = CalculateStat(StatType.HP, baseHP, floor);
            int atk = CalculateStat(StatType.ATK, baseATK, floor);
            int agl = CalculateStat(StatType.AGL, baseAGL, floor);

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

            int hp = CalculateStat(StatType.HP, baseHP, floor);
            int atk = CalculateStat(StatType.ATK, baseATK, floor);
            int agl = CalculateStat(StatType.AGL, baseAGL, floor);

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
        /// 階層とステータスタイプに応じた計算
        /// 式: Base * (1 + (階数 * 小増加率) + (階数/10 * 大増加率))
        /// </summary>
        private int CalculateStat(StatType type, int baseValue, int floor)
        {
            // 1階層目を基準（0）とするため -1 する
            int floorIndex = Mathf.Max(0, floor - 1);

            float growthPerFloor = 0f;
            float growthPer10Floors = 0f;

            // タイプごとに倍率を設定
            switch (type)
            {
                case StatType.HP:
                    growthPerFloor = hpGrowthPerFloor;
                    growthPer10Floors = hpGrowthPer10Floors;
                    break;
                case StatType.ATK:
                    growthPerFloor = atkGrowthPerFloor;
                    growthPer10Floors = atkGrowthPer10Floors;
                    break;
                case StatType.AGL:
                    growthPerFloor = aglGrowthPerFloor;
                    growthPer10Floors = aglGrowthPer10Floors;
                    break;
            }

            // 1フロアごとの微増分 (線形増加)
            float smallMultiplier = floorIndex * growthPerFloor;

            // 10フロアごとの大幅増分 (10階層ごとにガツンと上がる)
            // 整数除算 int / int で小数点以下切り捨てを利用 (例: 19/10 = 1, 20/10 = 2)
            float largeMultiplier = (floorIndex / 10) * growthPer10Floors;

            // 基本倍率1.0に加算
            float totalMultiplier = 1.0f + smallMultiplier + largeMultiplier;

            return Mathf.RoundToInt(baseValue * totalMultiplier);
        }
    }
}
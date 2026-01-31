using System.Collections;
using System.Collections.Generic;
using GGJ2026.Core.Managers;
using UnityEngine;

namespace GGJ2026.InGame.Enemy
{
    /// <summary>
    /// 敵を生成するファクトリクラス
    /// </summary>
    public class EnemyFactory : Singleton<EnemyFactory>
    {
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
        }

        public GameObject CreateEnemy(string enemyType)
        {
            // 敵の種類に応じて生成処理を実装
            GameObject enemy = new GameObject(enemyType);
            // 敵の初期化処理などをここに追加
            return enemy;
        }
    }
}

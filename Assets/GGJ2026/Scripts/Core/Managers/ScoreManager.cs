using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.Core;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// スコアを管理するマネージャー
    /// </summary>
    public class ScoreManager : Singleton<ScoreManager>
    {
        [SerializeField, ReadOnly] private int score = 0;
        public int Score => score;
        protected override bool UseDontDestroyOnLoad => true;
        
        public override void Init()
        {
            base.Init();
            ResetScore();
        }

        /// <summary>
        /// スコアを追加する
        /// </summary>
        /// <param name="value"></param>
        public void AddScore(int value) => score += value;

        /// <summary>
        /// スコアをリセットする
        /// </summary>
        public void ResetScore() => score = 0;

    }
}

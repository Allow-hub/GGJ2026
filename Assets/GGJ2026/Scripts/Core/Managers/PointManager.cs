using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.Core;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// ポイントを管理するマネージャー
    /// </summary>
    public class PointManager : Singleton<PointManager>
    {
        protected override bool UseDontDestroyOnLoad => true;
        [SerializeField, ReadOnly] private int points = 0;
        public int Points => points;
        public override void Init()
        {
            base.Init();
            ResetPoints();
        }

        [ContextMenu("Add 200 Points")]
        private void Add200Points()
        {
            AddPoints(200);
        }
        public void AddPoints(int value) => points += value;

        /// <summary>
        /// ポイントを使用する
        /// </summary>
        /// <param name="value">消費する値</param>
        /// <returns>ポイントが足りない場合はfalseを返す</returns>
        public bool UsePoints(int value)
        {
            if (points < value) return false;
            points -= value;
            return true;
        }
        public void ResetPoints() => points = 0;
    }
}

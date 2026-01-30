using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.Core.Managers;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// ゲーム全体を管理する最上位マネージャー
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameObject titleObj;
        [SerializeField] private GameObject inGameObj;

        protected override bool UseDontDestroyOnLoad => true;

        public override void Init()
        {
            base.Init();
        }
    }
}

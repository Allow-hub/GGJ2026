using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.Core.Managers;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// ゲーム中の管理を行う上位マネージャークラス
    /// </summary>
    public class InGameManager : Singleton<InGameManager>
    {
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
        }
    }
}
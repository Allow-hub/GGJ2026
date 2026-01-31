using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// 敵管理を行うマネージャークラス
    /// </summary>
    public class EnemyManager : Singleton<EnemyManager>
    {
        protected override bool UseDontDestroyOnLoad => false;

        public override void Init()
        {
            base.Init();
        }
    }
}

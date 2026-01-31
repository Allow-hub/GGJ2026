using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026.Interface
{
    /// <summary>
    /// 攻撃可能なオブジェクトのインターフェース
    /// </summary>
    public interface IAttackable
    {
        /// <summary>
        /// 攻撃力を取得
        /// </summary>
        int Damage { get; }

        /// <summary>
        /// 攻撃を実行
        /// </summary>
        /// <param name="target">攻撃対象</param>
        void Attack(IDamageable target);
    }
}

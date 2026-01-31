using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026.Interface
{
    /// <summary>
    /// ダメージを受けることができるオブジェクトのインターフェース
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">受けるダメージ量</param>
        void TakeDamage(int damage);
    }
}

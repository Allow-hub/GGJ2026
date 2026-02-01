using System.Collections;
using System.Collections.Generic;
using GGJ2026.Core.Managers;
using GGJ2026.InGame.Events;
using GGJ2026.Interface;
using TechC.VBattle.Core;
using UnityEngine;

namespace GGJ2026.InGame.Enemy
{
    /// <summary>
    /// 敵コントローラー
    /// IAttackableとIDamageableを実装
    /// </summary>
    public class EnemyController : MonoBehaviour, IAttackable, IDamageable
    {
        [Header("ステータス")]
        private int maxHP;
        public int MaxHP => maxHP;
        [SerializeField, ReadOnly] private int currentHP;
        public int CurrentHP => currentHP;
        private int atk;
        private int agl;
        private int floor;

        [Header("攻撃設定")]
        [SerializeField] private float baseAttackInterval = 2.0f; // デフォルトの攻撃インターバル（秒）
        private float attackTimer = 0f;
        private float currentAttackInterval;

        int IAttackable.Damage => atk;

        public bool IsAlive => currentHP > 0;

        /// <summary>
        /// 敵の初期化
        /// </summary>
        /// <param name="hp">最大HP</param>
        /// <param name="attackPower">攻撃力</param>
        /// <param name="agility">攻撃速度</param>
        /// <param name="currentFloor">現在の階層</param>
        public void Init(int hp, int attackPower, int agility, int currentFloor)
        {
            maxHP = hp;
            currentHP = maxHP;
            atk = attackPower;
            agl = agility;
            floor = currentFloor;

            // AGLに応じた攻撃インターバルを計算
            currentAttackInterval = CalculateAttackInterval(agl);
            attackTimer = currentAttackInterval; // 初回攻撃のタイマーをセット

            InGameManager.I.EventBus.Subscribe<AttackEvents>(e => CheckDamage(e));

            Debug.Log($"Enemy initialized on Floor {floor} - HP: {maxHP}, ATK: {atk}, AGL: {agl}, AttackInterval: {currentAttackInterval:F2}s");
        }

        private void Update()
        {
            if (!IsAlive)
                return;

            // 攻撃タイマーを更新
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                // 攻撃イベントをPublish
                Attack(InGameManager.I.PlayerController);

                // タイマーをリセット
                attackTimer = currentAttackInterval;
            }
        }

        /// <summary>
        /// AGLに応じた攻撃インターバルを計算
        /// AGLが高いほどインターバルが短くなる
        /// </summary>
        /// <param name="agility">攻撃速度</param>
        /// <returns>計算された攻撃インターバル（秒）</returns>
        private float CalculateAttackInterval(int agility)
        {
            // AGLが高いほどインターバルが短くなる
            // 例: AGL = 5 → interval = 2.0s
            //     AGL = 10 → interval = 1.0s
            //     AGL = 20 → interval = 0.5s
            float interval = baseAttackInterval / (agility / 5f);//5は調整用

            // 最小インターバルを設定（0.1秒）
            return Mathf.Max(interval, 0.1f);
        }

        /// <summary>
        /// 攻撃を実行
        /// </summary>
        /// <param name="target">攻撃対象</param>
        public void Attack(IDamageable target)
        {
            if (!IsAlive)
            {
                Debug.LogWarning("Dead enemy cannot attack");
                return;
            }

            if (target == null)
            {
                Debug.LogWarning("Attack target is null");
                return;
            }
            InGameManager.I.EventBus.Publish(new AttackEvents(this, InGameManager.I.PlayerController));

            Debug.Log($"攻撃イベントをPublish");
            // target.TakeDamage(atk);
        }

        /// <summary>
        /// ダメージが正しいかを判定
        /// </summary>
        /// <param name="attackEvents">攻撃のイベント</param>
        private void CheckDamage(AttackEvents attackEvents)
        {
            // 自分がターゲットじゃなければ無視
            if (!ReferenceEquals(attackEvents.Target, this)) return;
            // 攻撃者が null / 自分自身などのガード（任意）
            if (attackEvents.Attacker == null || ReferenceEquals(attackEvents.Attacker, this))
                return;

            TakeDamage(attackEvents.Attacker.Damage);
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">受けるダメージ量</param>
        public void TakeDamage(int damage)
        {
            if (!IsAlive)
                return;

            currentHP -= damage;
            currentHP = Mathf.Max(0, currentHP);

            // Debug.Log($"Enemy took {damage} damage. Remaining HP: {currentHP}/{maxHP}");

            if (!IsAlive)
                OnDeath();
        }

        /// <summary>
        /// 死亡時の処理
        /// </summary>
        [ContextMenu("OnDeath")]
        private void OnDeath()
        {
            Debug.Log($"Enemy defeated on Floor {floor}");
            EnemyFactory.I.DecrementEnemyCount(this);// 敵数を減らす
            // 死亡時の処理（アニメーション、ドロップなど）
            Destroy(gameObject);
        }
    }
}
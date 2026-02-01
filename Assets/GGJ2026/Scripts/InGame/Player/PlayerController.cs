using System.Collections;
using System.Collections.Generic;
using GGJ2026.Core.Managers;
using GGJ2026.Events;
using GGJ2026.InGame.Enemy;
using GGJ2026.InGame.Events;
using GGJ2026.Interface;
using Unity.Collections;
using UnityEngine;

namespace GGJ2026.InGame
{
    /// <summary>
    /// プレイヤーコントローラー
    /// Singletonを廃止し、EnemyControllerと同様の構成に変更
    /// </summary>
    public class PlayerController : MonoBehaviour, IAttackable, IDamageable
    {
        [Header("Player Stats")]
        [SerializeField] private int maxHp = 100;
        
        [SerializeField, ReadOnly] private int currentHp;
        [SerializeField] private float speed = 10f; 
        [SerializeField] private int attackPower = 5; 
        
        [Header("Growth Settings (LvUP時の上昇値)")] // ★追加
        [SerializeField] private int hpGrowthPerLevel = 20;
        [SerializeField] private int atkGrowthPerLevel = 2;
        [SerializeField] private float speedGrowthPerLevel = 1.0f;

        [Header("Attack Settings")]
        [SerializeField] private float baseAttackInterval = 2.0f; 
        [SerializeField] private CharacterSpriteAnimator characterSpriteAnimator;
        private float attackTimer = 0f;
        private float currentAttackInterval;

        // 外部公開用のプロパティ
        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public float Speed => speed;
        public int AttackPower => attackPower;

        // IAttackable 実装
        int IAttackable.Damage => attackPower;
        public bool IsAlive => currentHp > 0;
        
        private void Start()
        {
            Init();
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Init()
        {
            // HP初期化
            currentHp = maxHp;

            // 攻撃間隔の初期計算
            currentAttackInterval = CalculateAttackInterval(speed);
            attackTimer = currentAttackInterval;
            
            if (InGameManager.IsValid())
            {
                // パッシブスキル着脱イベント
                InGameManager.I.EventBus.Subscribe<InGameEvent.PassiveEffectEvent>(OnPassiveEffect);
                
                // ★追加: 強化イベントの購読
                InGameManager.I.EventBus.Subscribe<ImproveEvents>(OnImprove);
                InGameManager.I.EventBus.Subscribe<AttackEvents>(CheckDamage);

                InGameManager.I.EventBus.Subscribe<InGameEvent.ApplyMainMaskEvent>(e => characterSpriteAnimator.ChangeMask(e.SelectedItem.Config.itemName));
            }

            //Debug.Log($"Player Initialized. HP: {currentHp}, ATK: {attackPower}, SPD: {speed}, Interval: {currentAttackInterval:F2}s");
        }
        
        private void OnDestroy()
        {
            if (InGameManager.IsValid())
            {
                InGameManager.I.EventBus.Unsubscribe<InGameEvent.PassiveEffectEvent>(OnPassiveEffect);
                InGameManager.I.EventBus.Unsubscribe<ImproveEvents>(OnImprove);
                InGameManager.I.EventBus.Unsubscribe<AttackEvents>(CheckDamage);
            }
        }


        
        private void CheckDamage(AttackEvents attackEvents)
        {
            // ターゲットが自分(Player)でなければ無視
            if (!ReferenceEquals(attackEvents.Target, this)) return;
            
            // 攻撃者が自分なら無視（念のため）
            if (ReferenceEquals(attackEvents.Attacker, this)) return;

            // ダメージを受ける
            TakeDamage(attackEvents.Attacker.Damage);
        }
        
        private void OnImprove(ImproveEvents e)
        {

            switch (e.playerParam)
            {
                case PlayerParam.Health:
                    maxHp += hpGrowthPerLevel;
                    currentHp += hpGrowthPerLevel; // 最大値が増えた分、現在HPも回復させてあげる（仕様次第）
                    Debug.Log($"[Improve] MaxHP Up! {maxHp} (+{hpGrowthPerLevel})");
                    break;

                case PlayerParam.AttackPower:
                    attackPower += atkGrowthPerLevel;
                    Debug.Log($"[Improve] Attack Up! {attackPower} (+{atkGrowthPerLevel})");
                    break;

                case PlayerParam.Agility:
                    speed += speedGrowthPerLevel;
                    // Speedが変わったら攻撃間隔を再計算
                    currentAttackInterval = CalculateAttackInterval(speed);
                    Debug.Log($"[Improve] Speed Up! {speed} (+{speedGrowthPerLevel})");
                    break;
            }
        }

        private void Update()
        {
            if (!IsAlive) return;
            

            if (InGameManager.IsValid() && InGameManager.I.CurrentState != InGameState.Battle) return;

            // 攻撃タイマー更新
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                PublishAttackEvent();
                attackTimer = currentAttackInterval; // タイマーリセット
            }
        }

        /// <summary>
        /// Speed(AGL)に応じた攻撃インターバルを計算
        /// </summary>
        private float CalculateAttackInterval(float agility)
        {
            if (agility <= 0) return baseAttackInterval;
            float interval = baseAttackInterval / (agility / 5f);
            return Mathf.Max(interval, 0.1f); // 最小0.1秒
        }

        /// <summary>
        /// 攻撃イベントを発行
        /// </summary>
        private void PublishAttackEvent()
        {
            if (InGameManager.IsValid() && InGameManager.I.EventBus != null)
            {
                characterSpriteAnimator.PlayAttack();
                // 必要であればここでEventBusにPublish
                InGameManager.I.EventBus.Publish(new AttackEvents(this, EnemyFactory.I.CurrentEnemies[0]));
            }

            //Debug.Log($"Player attacks! (Damage: {attackPower})");
        }

        // IAttackable 実装
        public void Attack(IDamageable target)
        {
            if (!IsAlive || target == null) return;
            Debug.Log($"Player attacks target for {attackPower} damage");
            target.TakeDamage(attackPower);
        }

        // IDamageable 実装
        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;

            currentHp -= damage;
            if (currentHp < 0) currentHp = 0;

            //Debug.Log($"Player Took Damage: {damage}, CurrentHP: {currentHp}");

            if (currentHp == 0)
            {
                OnPlayerDead();
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;

            currentHp += amount;
            if (currentHp > maxHp) currentHp = maxHp;

            Debug.Log($"Player Healed: {amount}, CurrentHP: {currentHp}");
        }

        private void OnPlayerDead()
        {
            Debug.Log("Player is Dead.");
            if (InGameManager.IsValid())
            {
                InGameManager.I.EndGame(); 
            }
        }
        
        private void OnPassiveEffect(InGameEvent.PassiveEffectEvent e)
        {
            ApplyPassiveEffect(e.Skill, e.IsEquip);
        }

        /// <summary>
        /// パッシブスキルの適用/解除
        /// </summary>
        public void ApplyPassiveEffect(PassiveSkillInstance skill, bool isEquip)
        {
            if (skill == null) return;

            float value = isEquip ? skill.Value : -skill.Value;

            switch (skill.Config._modifierType)
            {
                case ModifierType.PlayerParam:
                    ApplyPlayerParam(skill.Config._targetPlayerParam, value);
                    break;
            }
        }

        private void ApplyPlayerParam(PlayerParam param, float value)
        {
            switch (param)
            {
                case PlayerParam.Health:
                    maxHp += (int)value;
                    // MaxHP減少時に現在HPが溢れないようにする
                    if (currentHp > maxHp) currentHp = maxHp;
                    Debug.Log($"MaxHP Changed: {maxHp} ({value})");
                    break;

                case PlayerParam.AttackPower:
                    attackPower += (int)value;
                    Debug.Log($"AttackPower Changed: {attackPower} ({value})");
                    break;

                case PlayerParam.Agility:
                    speed += value;
                    // Speedが変わったら攻撃間隔を再計算
                    currentAttackInterval = CalculateAttackInterval(speed);
                    Debug.Log($"Speed Changed: {speed} ({value}), New Interval: {currentAttackInterval:F2}s");
                    break;
            }
        }
    }
}
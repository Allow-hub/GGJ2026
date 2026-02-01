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
    public class PlayerController : MonoBehaviour, IAttackable, IDamageable
    {
        [Header("Player Stats")]
        [SerializeField] private int maxHp = 100;
        
        [SerializeField, ReadOnly] private int currentHp;
        [SerializeField] private float speed = 10f; 
        [SerializeField] private int attackPower = 5; 
        
        [Header("Growth Settings (LvUP時の上昇値)")]
        [SerializeField] private int hpGrowthPerLevel = 20;
        [SerializeField] private int atkGrowthPerLevel = 2;
        [SerializeField] private float speedGrowthPerLevel = 1.0f;

        [Header("Attack Settings")]
        [SerializeField] private float baseAttackInterval = 2.0f; 
        [SerializeField] private CharacterSpriteAnimator characterSpriteAnimator;
        private float attackTimer = 0f;
        private float currentAttackInterval;

        // ★追加: 現在適用中のメインスキル（アクティブスキル）を保持
        private ActiveSkillInstance currentMainSkill;

        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public float Speed => speed;
        public int AttackPower => attackPower;

        int IAttackable.Damage => attackPower;
        public bool IsAlive => currentHp > 0;
        
        private void Start()
        {
            Init();
        }
        
        public void Init()
        {
            currentHp = maxHp;
            currentAttackInterval = CalculateAttackInterval(speed);
            attackTimer = currentAttackInterval;
            currentMainSkill = null; // 初期化
            
            if (InGameManager.IsValid())
            {
                InGameManager.I.EventBus.Subscribe<InGameEvent.PassiveEffectEvent>(OnPassiveEffect);
                InGameManager.I.EventBus.Subscribe<ImproveEvents>(OnImprove);
                InGameManager.I.EventBus.Subscribe<AttackEvents>(CheckDamage);

                // メインマスク適用イベントの購読
                InGameManager.I.EventBus.Subscribe<InGameEvent.ApplyMainMaskEvent>(OnApplyMainMask);
            }
        }
        
        private void OnDestroy()
        {
            if (InGameManager.IsValid())
            {
                InGameManager.I.EventBus.Unsubscribe<InGameEvent.PassiveEffectEvent>(OnPassiveEffect);
                InGameManager.I.EventBus.Unsubscribe<ImproveEvents>(OnImprove);
                InGameManager.I.EventBus.Unsubscribe<AttackEvents>(CheckDamage);
                InGameManager.I.EventBus.Unsubscribe<InGameEvent.ApplyMainMaskEvent>(OnApplyMainMask);
            }
        }

        private void CheckDamage(AttackEvents attackEvents)
        {
            if (!ReferenceEquals(attackEvents.Target, this)) return;
            if (ReferenceEquals(attackEvents.Attacker, this)) return;
            TakeDamage(attackEvents.Attacker.Damage);
        }
        
        private void OnImprove(ImproveEvents e)
        {
            switch (e.playerParam)
            {
                case PlayerParam.Health:
                    maxHp += hpGrowthPerLevel;
                    currentHp += hpGrowthPerLevel;
                    Debug.Log($"[Improve] MaxHP Up! {maxHp} (+{hpGrowthPerLevel})");
                    break;
                case PlayerParam.AttackPower:
                    attackPower += atkGrowthPerLevel;
                    Debug.Log($"[Improve] Attack Up! {attackPower} (+{atkGrowthPerLevel})");
                    break;
                case PlayerParam.Agility:
                    speed += speedGrowthPerLevel;
                    currentAttackInterval = CalculateAttackInterval(speed);
                    Debug.Log($"[Improve] Speed Up! {speed} (+{speedGrowthPerLevel})");
                    break;
            }
        }

        private void Update()
        {
            if (!IsAlive) return;
            if (InGameManager.IsValid() && InGameManager.I.CurrentState != InGameState.Battle) return;

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                PublishAttackEvent();
                attackTimer = currentAttackInterval;
            }
        }

        private float CalculateAttackInterval(float agility)
        {
            if (agility <= 0) return baseAttackInterval;
            float interval = baseAttackInterval / (agility / 5f);
            return Mathf.Max(interval, 0.1f);
        }

        /// <summary>
        /// 攻撃イベントを発行
        /// </summary>
        private void PublishAttackEvent()
        {
            if (InGameManager.IsValid() && InGameManager.I.EventBus != null)
            {
                characterSpriteAnimator.PlayAttack();
                AudioManager.I.PlaySE(Core.Audio.SEID.Attack);
                // 必要であればここでEventBusにPublish

                // ★修正: Attackスキルの場合、一時的に攻撃力を上げる
                int originalAttackPower = attackPower; // 元の攻撃力を保存
                bool isBuffed = false;

                if (currentMainSkill != null && currentMainSkill.Config.effectTarget == EffectTarget.Attack)
                {
                    // 確率判定
                    float randomValue = UnityEngine.Random.Range(0f, 100f);
                    if (randomValue <= currentMainSkill.Config.probability)
                    {
                        // 倍率適用
                        attackPower = Mathf.RoundToInt(attackPower * currentMainSkill.Config.multipler);
                        isBuffed = true;
                        Debug.Log($"[MainSkill] Attack Boost! {originalAttackPower} -> {attackPower}");
                    }
                }

                // イベント発行 (この時点で attackPower は強化されている可能性がある)
                InGameManager.I.EventBus.Publish(new AttackEvents(this, EnemyFactory.I.CurrentEnemies[0]));

                // ★修正: 攻撃が終わったら元の攻撃力に戻す
                if (isBuffed)
                {
                    attackPower = originalAttackPower;
                }
            }
        }

        public void Attack(IDamageable target)
        {
            if (!IsAlive || target == null) return;
            Debug.Log($"Player attacks target for {attackPower} damage");
            target.TakeDamage(attackPower);
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;

            // ★修正: Defenceスキルの場合、ダメージを0.7倍にする
            if (currentMainSkill != null && currentMainSkill.Config.effectTarget == EffectTarget.Defence)
            {
                int originalDmg = damage;
                damage = Mathf.RoundToInt(damage * 0.7f);
                Debug.Log($"[MainSkill] Damage Reduced: {originalDmg} -> {damage}");
            }

            currentHp -= damage;
            if (currentHp < 0) currentHp = 0;
            if (currentHp == 0) OnPlayerDead();
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
                    if (currentHp > maxHp) currentHp = maxHp;
                    Debug.Log($"MaxHP Changed: {maxHp} ({value})");
                    break;
                case PlayerParam.AttackPower:
                    attackPower += (int)value;
                    Debug.Log($"AttackPower Changed: {attackPower} ({value})");
                    break;
                case PlayerParam.Agility:
                    speed += value;
                    currentAttackInterval = CalculateAttackInterval(speed);
                    Debug.Log($"Speed Changed: {speed} ({value}), New Interval: {currentAttackInterval:F2}s");
                    break;
                case PlayerParam.Recovery:
                    currentHp = maxHp;
                    Debug.Log($"Player HP Fully Recovered: {currentHp}/{maxHp}");
                    break;
            }
        }

        // -----------------------------------------------------------------------
        // メインマスク(アクティブスキル)関連
        // -----------------------------------------------------------------------

        private void OnApplyMainMask(InGameEvent.ApplyMainMaskEvent e)
        {
            if (e.SelectedItem != null && e.SelectedItem.ActiveSkill != null)
            {
                // 現在のスキルを更新
                currentMainSkill = e.SelectedItem.ActiveSkill;
                
                // 静的な効果（Time, Point）のみここで適用
                ApplyMainSkillStaticEffects(currentMainSkill);
            }
        }

        /// <summary>
        /// メインスキルのうち、装着時に一度だけ適用される効果 (Time, Point)
        /// Attack, Defence は UpdateやTakeDamageで都度判定するためここでは何もしない
        /// </summary>
        private void ApplyMainSkillStaticEffects(ActiveSkillInstance skillInstance)
        {
            ActiveSkillConfig config = skillInstance.Config;
            float multiplier = config.multipler;

            Debug.Log($"[MainMask] Skill Set: {config.skillName} (Target: {config.effectTarget})");

            switch (config.effectTarget)
            {
                case EffectTarget.Attack:
                    // ここでは何もしない（攻撃時に確率発動）
                    break;

                case EffectTarget.Defence:
                    // ここでは何もしない（被ダメージ時に0.7倍）
                    break;

                case EffectTarget.Time:
                    // ★修正: 確率で残り時間を5秒追加
                    if (InGameManager.I != null)
                    {
                        float randomValue = UnityEngine.Random.Range(0f, 100f);
                        if (randomValue <= config.probability)
                        {
                            InGameManager.I.CurrentTime += 5f;
                            Debug.Log($"[MainMask] Time Extended: +5.0s (Chance: {config.probability}%)");
                        }
                        else
                        {
                            Debug.Log($"[MainMask] Time Extension Failed (Chance: {config.probability}%)");
                        }
                    }
                    break;

                case EffectTarget.Point:
                    // ポイント獲得（そのまま）
                    if (InGameManager.I != null)
                    {
                        float randomValue = UnityEngine.Random.Range(0f, 100f);
                        if (randomValue > config.probability)
                        {
                            InGameManager.I.SetPointMultiplier(multiplier);
                            Debug.Log($"[MainMask] Points Add: {multiplier}");
                        }
                    }
                    break;
            }
        }
    }
}
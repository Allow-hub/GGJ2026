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
        
        // ★修正: パーティクルではなく、スプライトを使ったフラッシュ演出に変更
        [Header("Effects")]
        [SerializeField] private SpriteRenderer mainSpriteRenderer; // プレイヤー自身のSpriteRenderer
        [SerializeField] private SpriteRenderer flashSpriteRenderer; // 背後に配置する演出用SpriteRenderer
        [SerializeField] private float flashDuration = 0.15f; // 光る時間
        [SerializeField] private Color flashColor = new Color(1f, 1f, 0.7f, 1f); // 光の色（薄い黄色など）
        [SerializeField] private Vector3 flashScale = new Vector3(1.2f, 1.2f, 1f); // どれくらい膨張させるか
        
        [Header("UI Effects")]
        [SerializeField] private GameObject damagePopupPrefab;
        [SerializeField] private Vector3 popupOffset = new Vector3(0, 1.5f, 0);

        private float attackTimer = 0f;
        private float currentAttackInterval;

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
            currentMainSkill = null;
            
            // フラッシュ用スプライトは最初は非表示にしておく
            if (flashSpriteRenderer != null)
            {
                flashSpriteRenderer.gameObject.SetActive(false);
            }

            if (InGameManager.IsValid())
            {
                InGameManager.I.EventBus.Subscribe<InGameEvent.PassiveEffectEvent>(OnPassiveEffect);
                InGameManager.I.EventBus.Subscribe<ImproveEvents>(OnImprove);
                InGameManager.I.EventBus.Subscribe<AttackEvents>(CheckDamage);
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
                case PlayerParam.Recovery:
                    currentHp = maxHp;
                    Debug.Log($"[Improve] Full Recovery! HP: {currentHp}/{maxHp}");
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

                // ★追加: 背後のフラッシュ演出を開始
                if (flashSpriteRenderer != null && mainSpriteRenderer != null)
                {
                    // 以前のコルーチンが走っていたら止めて、新しく再生
                    StopCoroutine(nameof(PlayFlashEffect));
                    StartCoroutine(nameof(PlayFlashEffect));
                }

                // Attackスキルの場合、一時的に攻撃力を上げる
                int originalAttackPower = attackPower;
                bool isBuffed = false;

                if (currentMainSkill != null && currentMainSkill.Config.effectTarget == EffectTarget.Attack)
                {
                    float randomValue = UnityEngine.Random.Range(0f, 100f);
                    if (randomValue <= currentMainSkill.Config.probability)
                    {
                        attackPower = Mathf.RoundToInt(attackPower * currentMainSkill.Config.multipler);
                        isBuffed = true;
                        Debug.Log($"[MainSkill] Attack Boost! {originalAttackPower} -> {attackPower}");
                    }
                }

                InGameManager.I.EventBus.Publish(new AttackEvents(this, EnemyFactory.I.CurrentEnemies[0]));

                if (isBuffed)
                {
                    attackPower = originalAttackPower;
                }
            }
        }

        private IEnumerator PlayFlashEffect()
        {
            flashSpriteRenderer.gameObject.SetActive(true);
            
            flashSpriteRenderer.sprite = mainSpriteRenderer.sprite;
            flashSpriteRenderer.flipX = mainSpriteRenderer.flipX;
            
            // 初期サイズ
            flashSpriteRenderer.transform.localScale = flashScale;

            // ★変更点: 設定されている色のアルファ値（透明度）を最大値として使う
            float maxAlpha = flashColor.a; 
            
            float timer = 0f;

            while (timer < flashDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / flashDuration; // 0 から 1 へ進む

                // maxAlpha から 0 へ向かって薄くしていく
                float currentAlpha = Mathf.Lerp(maxAlpha, 0f, progress);
                
                Color c = flashColor;
                c.a = currentAlpha;
                flashSpriteRenderer.color = c;

                yield return null;
            }

            flashSpriteRenderer.gameObject.SetActive(false);
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

            // Defenceスキルの軽減処理
            if (currentMainSkill != null && currentMainSkill.Config.effectTarget == EffectTarget.Defence)
            {
                int originalDmg = damage;
                damage = Mathf.RoundToInt(damage * 0.7f);
                Debug.Log($"[MainSkill] Damage Reduced: {originalDmg} -> {damage}");
            }

            // ★追加: ダメージポップアップの表示
            if (damagePopupPrefab != null)
            {
                // プレイヤーの少し左上あたりから出現させると「左から右へ」が綺麗に見えます
                Vector3 spawnPos = transform.position + popupOffset + new Vector3(-0.5f, 0, 0);
                
                GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
                DamagePopup damagePopup = popup.GetComponent<DamagePopup>();
                if (damagePopup != null)
                {
                    damagePopup.Setup(damage);
                }
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

        private void OnApplyMainMask(InGameEvent.ApplyMainMaskEvent e)
        {
            if (e.SelectedItem != null && e.SelectedItem.ActiveSkill != null)
            {
                currentMainSkill = e.SelectedItem.ActiveSkill;
                ApplyMainSkillStaticEffects(currentMainSkill);
            }
        }

        private void ApplyMainSkillStaticEffects(ActiveSkillInstance skillInstance)
        {
            ActiveSkillConfig config = skillInstance.Config;
            float multiplier = config.multipler;

            Debug.Log($"[MainMask] Skill Set: {config.skillName} (Target: {config.effectTarget})");

            switch (config.effectTarget)
            {
                case EffectTarget.Attack:
                    break;

                case EffectTarget.Defence:
                    break;

                case EffectTarget.Time:
                    if (InGameManager.I != null)
                    {
                        float randomValue = UnityEngine.Random.Range(0f, 100f);
                        if (randomValue <= config.probability)
                        {
                            InGameManager.I.CurrentTime += 5f;
                            Debug.Log($"[MainMask] Time Extended: +5.0s (Chance: {config.probability}%)");
                        }
                    }
                    break;

                case EffectTarget.Point:
                    if (InGameManager.I != null)
                    {
                        float randomValue = UnityEngine.Random.Range(0f, 100f);
                        if (randomValue <= config.probability)
                        {
                            InGameManager.I.SetPointMultiplier(multiplier);
                            Debug.Log($"[MainMask] Points Multiplier Set: {multiplier}");
                        }
                    }
                    break;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using GGJ2026.Core.Managers; // Singleton<T>のために必要
using GGJ2026.InGame.Events;
using GGJ2026.Interface;
using TechC.VBattle.Core;
using UnityEngine;

namespace GGJ2026.InGame
{
    /// <summary>
    /// プレイヤーコントローラー
    /// </summary>
    // ★ここを修正しました
    public class PlayerController : Singleton<PlayerController>, IAttackable, IDamageable
    {
        [Header("Player Stats")]
        [SerializeField] private int maxHp = 100;
        [SerializeField] private float speed = 10f; 
        [SerializeField] private int attackPower = 5; 

        [Header("Attack Settings")]
        [SerializeField] private float baseAttackInterval = 2.0f; 
        private float attackTimer = 0f;
        private float currentAttackInterval;

        public int CurrentHp { get; private set; }
        public float Speed => speed;
        public int AttackPower => attackPower;

        int IAttackable.Damage => attackPower;
        public bool IsAlive => CurrentHp > 0;
        
        // シーン遷移で破棄するため false (Singleton<T>のオーバーライド)
        protected override bool UseDontDestroyOnLoad => false;

        // 明示的な初期化 (Singleton<T>の仕様)
        private new void Awake()
        {
            InitializeSingleton();
        }

        public override void Init()
        {
            base.Init(); // 基底クラスのInitも呼ぶ

            // HP初期化
            CurrentHp = maxHp;

            // 攻撃間隔の初期計算
            currentAttackInterval = CalculateAttackInterval(speed);
            attackTimer = currentAttackInterval;

            Debug.Log($"Player Initialized. HP: {CurrentHp}, ATK: {attackPower}, SPD: {speed}, Interval: {currentAttackInterval:F2}s");
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

        private void PublishAttackEvent()
        {
            Debug.Log($"Player attacks! (Damage: {attackPower})");
        }

        public void Attack(IDamageable target)
        {
            if (!IsAlive || target == null) return;
            target.TakeDamage(attackPower);
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;

            CurrentHp -= damage;
            if (CurrentHp < 0) CurrentHp = 0;

            Debug.Log($"Player Took Damage: {damage}, CurrentHP: {CurrentHp}");

            if (CurrentHp == 0)
            {
                OnPlayerDead();
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;

            CurrentHp += amount;
            if (CurrentHp > maxHp) CurrentHp = maxHp;

            Debug.Log($"Player Healed: {amount}, CurrentHP: {CurrentHp}");
        }

        private void OnPlayerDead()
        {
            Debug.Log("Player is Dead.");
            if (InGameManager.IsValid())
            {
                // InGameManager.I.OnPlayerDead(); 
            }
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
                    if (CurrentHp > maxHp) CurrentHp = maxHp;
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
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 250, 250), "Debug: Player Status", GUI.skin.window);

            GUILayout.Label($"HP: {CurrentHp} / {maxHp}");
            GUILayout.Label($"Speed: {Speed}");
            GUILayout.Label($"Interval: {currentAttackInterval:F2}s");
            GUILayout.Label($"Attack: {AttackPower}");

            GUILayout.Space(10);

            if (GUILayout.Button("Damage 10")) TakeDamage(10);
            if (GUILayout.Button("Heal 10")) Heal(10);

            GUILayout.EndArea();
        }
    }
}
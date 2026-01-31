using UnityEngine;

namespace GGJ2026.InGame
{
    [CreateAssetMenu(fileName = "NewActiveSkill", menuName = "AddSO/ActiveSkill")]
    public class ActiveSkillConfig:ScriptableObject
    {
        [Header("スキル名")]
        public string skillName;
        [Header("説明")]
        public string description;
        [Header("対象")]
        public EffectTarget effectTarget;
        [Header("倍率")]
        public float multipler;
        [Tooltip("発動確率")]
        [Range(0, 100)]
        public float probability = 100f;
    }
    
    public enum EffectTarget
    {
        Point,
        Defence,
        Attack,
        Time,
    }
}
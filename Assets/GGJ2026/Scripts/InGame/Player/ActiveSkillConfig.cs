using UnityEngine;

namespace GGJ2026.InGame
{
    [CreateAssetMenu(fileName = "NewActiveSkill", menuName = "AddSO/ActiveSkill")]
    public abstract class ActiveSkillConfig:ScriptableObject
    {
        [Header("スキル名")]
        public string skillName;
        [Header("説明")]
        public string description;
        [Header("クールタイム")]
        public float skillCoolTime;
    }
}
using UnityEngine;

namespace GGJ2026.InGame
{
    [CreateAssetMenu(fileName = "NewPassiveSkill", menuName = "AddSO/PassiveSkill")]
    public class PassiveSkillConfig: ScriptableObject
    {
        [Header("表示情報")]
        public string _skillName;
        
        [Tooltip("抽選時の重み (大きいほど出やすい相対評価)")]
        [SerializeField, Range(1, 100)] public int baseWeight = 10;

        [Header("ステータス対象")]
        public ModifierType _modifierType; // ステータス対象種別
        public ItemParam _targetItemParam;
        public PlayerParam _targetPlayerParam;
        public ActiveSkillParam _targetActiveSkillParam;
        public GameRuleParam _targetGameRuleParam;

        [Header("ランダム生成の幅")]
        public float _minValue;
        public float _maxValue;
    }

    public enum ModifierType
    {
        None,
        ItemParam,
        ActiveSkillParam,
        PlayerParam,
        GameRuleParam
    }

    public enum ItemParam
    {
        Price
    }
    
    public enum PlayerParam
    {
        Health,
        AttackPower,
        Agility,
        Recovery
    }

    public enum ActiveSkillParam
    {
        SkillCoolTime,
    }

    public enum GameRuleParam
    {
        GameSpeed,
        BasePointRate,
    }
}
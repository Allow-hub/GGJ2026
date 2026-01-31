

using UnityEngine;

namespace GGJ2026.InGame
{
    public class PassiveSkillInstance
    {
        public PassiveSkillConfig Config { get; private set; }
        public float Value { get; private set; }

        public PassiveSkillInstance(PassiveSkillConfig config)
        {
            Config = config;
            Value = Random.Range(config._minValue, config._maxValue);
        }

        public string GetDescription()
        {
            return $"{Config._skillName}: {Config._modifierType} {Value:F1}";
        }
    }
    
    public class ActiveSkillInstance
    {
        public ActiveSkillConfig Config { get; private set; }
        private float currentMultipler;
        private float currentprobability;

        public ActiveSkillInstance(ActiveSkillConfig config)
        {
            Config = config;
            currentMultipler = config.multipler;
            currentprobability = config.probability;
        }
    }
    
    public class ItemInstance
    {
        public ItemConfig Config { get; private set; }
        public ActiveSkillInstance ActiveSkill { get; private set; }
        
        public PassiveSkillInstance PassiveSkill { get; private set; }

        public ItemInstance(ItemConfig config)
        {
            Config = config;
        }

        public void SetActiveSkill(ActiveSkillInstance skill)
        {
            ActiveSkill = skill;
        }
        
        public void SetPassiveSkill(PassiveSkillInstance skill)
        {
            PassiveSkill = skill;
        }
    }
}
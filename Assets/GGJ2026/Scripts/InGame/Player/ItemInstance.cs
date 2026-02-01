using UnityEngine;

namespace GGJ2026.InGame
{
    public class PassiveSkillInstance
    {
        public PassiveSkillConfig Config { get; private set; }
        public float Value { get; private set; }

        public PassiveSkillInstance(PassiveSkillConfig config, int floor, float growthRate)
        {
            Config = config;

            float baseValue = Random.Range(config._minValue, config._maxValue);

            int floorIndex = Mathf.Max(0, floor - 1);
            float multiplier = 1.0f + (floorIndex * growthRate);

            Value = baseValue * multiplier;
        }

        public string GetDescription()
        {
            return $"{Config._skillName}: \n+ {Value:F1}";
        }
    }
    
    public class ActiveSkillInstance
    {
        public ActiveSkillConfig Config { get; private set; }
        
        // ★修正: 外部から参照できるように public プロパティに変更
        public float CurrentMultiplier { get; private set; }
        public float CurrentProbability { get; private set; }

        public ActiveSkillInstance(ActiveSkillConfig config)
        {
            Config = config;
            
            // ★修正: -10 ～ +10 のランダムな振れ幅を加算
            float multiplierVariation = Random.Range(-10f, 10f);
            float probabilityVariation = Random.Range(-10f, 10f);

            // ベース値に変動値を加える
            // ※確率は 0～100 の範囲に収まるように Clamp しておくと安全です
            // ※倍率もマイナスにならないように Max(0, ...) しておくと安全です
            
            CurrentMultiplier = Mathf.Max(0f, config.multipler + multiplierVariation);
            CurrentProbability = Mathf.Clamp(config.probability + probabilityVariation, 0f, 100f);
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
using UnityEngine;

namespace GGJ2026.InGame
{
    public class PassiveSkillInstance
    {
        public PassiveSkillConfig Config { get; private set; }
        public float Value { get; private set; }

        // ★修正: floor と growthRate を受け取る
        public PassiveSkillInstance(PassiveSkillConfig config, int floor, float growthRate)
        {
            Config = config;

            // 1. ベースとなるランダム値を決定
            float baseValue = Random.Range(config._minValue, config._maxValue);

            // 2. 階層補正を計算 (1階層目は補正なし)
            // floor が 1 未満にならないように Max(1, floor) を使用
            int floorIndex = Mathf.Max(0, floor - 1);
            float multiplier = 1.0f + (floorIndex * growthRate);

            // 3. 最終値を計算
            Value = baseValue * multiplier;
            
            // もし整数で扱いたいパラメータの場合は RoundToInt などしてください
            // Value = Mathf.Round(Value); 
        }

        public string GetDescription()
        {
            // F1 = 小数点第1位まで表示
            return $"{Config._skillName}: \n+ {Value:F1}";
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
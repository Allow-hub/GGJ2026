

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
            // 生成時にランダムな値（補正値）を決定
            Value = Random.Range(config._minValue, config._maxValue);
        }

        public string GetDescription()
        {
            return $"{Config._skillName}: {Config._modifierType} +{Value:F1}";
        }
    }

    // アクティブスキル実体（変更なし）
    public class ActiveSkillInstance
    {
        public ActiveSkillConfig Config { get; private set; }
        private float currentCoolTime;

        public ActiveSkillInstance(ActiveSkillConfig config)
        {
            Config = config;
            currentCoolTime = 0f;
        }

        public bool IsReady => currentCoolTime <= 0f;

        public void UpdateCooldown(float deltaTime)
        {
            if (currentCoolTime > 0f) currentCoolTime -= deltaTime;
        }

        public void Use()
        {
            currentCoolTime = Config.skillCoolTime;
            Debug.Log($"Used Skill: {Config.skillName}");
        }
    }

    // アイテム実体（★ここを修正）
    public class ItemInstance
    {
        public ItemConfig Config { get; private set; }
        public ActiveSkillInstance ActiveSkill { get; private set; }

        // ★修正: リストをやめて、1つだけ持つように変更
        public PassiveSkillInstance PassiveSkill { get; private set; }

        public ItemInstance(ItemConfig config)
        {
            Config = config;
        }

        public void SetActiveSkill(ActiveSkillInstance skill)
        {
            ActiveSkill = skill;
        }

        // ★修正: 追加(Add)ではなくセット(Set)に変更
        public void SetPassiveSkill(PassiveSkillInstance skill)
        {
            PassiveSkill = skill;
        }
    }
}
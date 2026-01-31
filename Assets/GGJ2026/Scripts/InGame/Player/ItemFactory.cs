using System.Collections.Generic;
using System.Linq; // 重み抽選に必要
using GGJ2026.Core.Managers; // Singletonの場所に合わせて変更してください
using UnityEngine;

namespace GGJ2026.InGame
{
public class ItemFactory : Singleton<ItemFactory>
    {
        protected override bool UseDontDestroyOnLoad => false;

        // 抽選候補となるパッシブスキルの全リスト
        [SerializeField] private List<PassiveSkillConfig> allPassiveSkillPool;

        private new void Awake()
        {
            InitializeSingleton();
        }

        /// <summary>
        /// アイテムを生成する
        /// </summary>
        /// <param name="config">ベースのアイテムデータ</param>
        /// <param name="useRandomPassive">trueならランダムにパッシブを1つ抽選して付ける。falseならConfigの固定パッシブを使う。</param>
        public ItemInstance CreateItem(ItemConfig config)
        {
            bool useRandomPassive = true;
            if (config == null) return null;

            var instance = new ItemInstance(config);

            // 1. アクティブスキルの生成
            if (config.activeSkill != null)
            {
                instance.SetActiveSkill(new ActiveSkillInstance(config.activeSkill));
            }

            // 2. パッシブスキルの決定（★修正: どちらか1つだけ）
            if (useRandomPassive)
            {
                // ランダムモード: プールから抽選してセット（Configの設定は無視）
                var randomConfig = GetRandomPassiveConfigFromPool();
                if (randomConfig != null)
                {
                    instance.SetPassiveSkill(new PassiveSkillInstance(randomConfig));
                }
            }
            else if (config.passiveSkills != null)
            {
                // 通常モード: Configに設定されている固定パッシブがあればセット
                instance.SetPassiveSkill(new PassiveSkillInstance(config.passiveSkills));
            }

            return instance;
        }

        /// <summary>
        /// 重み付き抽選
        /// </summary>
        private PassiveSkillConfig GetRandomPassiveConfigFromPool()
        {
            if (allPassiveSkillPool == null || allPassiveSkillPool.Count == 0) return null;

            int totalWeight = allPassiveSkillPool.Sum(x => x.baseWeight);
            int randomValue = Random.Range(0, totalWeight);

            int currentWeight = 0;
            foreach (var skill in allPassiveSkillPool)
            {
                currentWeight += skill.baseWeight;
                if (randomValue < currentWeight)
                {
                    return skill;
                }
            }

            return allPassiveSkillPool.Last();
        }
    }
}
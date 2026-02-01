using System.Collections.Generic;
using System.Linq; 
using GGJ2026.Core.Managers; 
using UnityEngine;

namespace GGJ2026.InGame
{ 
    public class ItemFactory : Singleton<ItemFactory>
    {
        protected override bool UseDontDestroyOnLoad => false;
        
        [Header("アイテムデータリスト")]
        [SerializeField] private List<ItemConfig> allItemConfig;
        
        [Header("パッシブスキルリスト")]
        [SerializeField] private List<PassiveSkillConfig> allPassiveSkillPool;
        
        [Header("参照")]
        [SerializeField] private GridView gridView; // 配置ロジックを持つGridViewへの参照
        
        // ★修正: 配置はGridViewに任せるため、ここでのContainerやRadius設定は削除しました

        private new void Awake()
        {
            InitializeSingleton();
        }
        
        /// <summary>
        /// アイテムを生成して配置する（配置場所はGridView任せ）
        /// </summary>
        public void SpawnItem(ItemInstance instance)
        {
            if (gridView != null)
            {
                // GridViewに生成と配置を依頼する
                // (-1, -1) を渡すことで「グリッド外（未装備）」として扱わせる
                gridView.SpawnItem(instance, -1, -1);
            }
            else
            {
                Debug.LogError("ItemFactory: GridView の参照が設定されていません！Inspectorを確認してください。");
            }
        }

        public ItemInstance ChooseItem()
        {
            if (allItemConfig == null || allItemConfig.Count == 0)
            {
                Debug.LogWarning("ItemFactory: 抽選用のItemConfigリストが空です。");
                return null;
            }
            int index = Random.Range(0, allItemConfig.Count);
            ItemConfig selectedConfig = allItemConfig[index];

            return CreateItem(selectedConfig);
        }

        public ItemInstance CreateItem(ItemConfig config)
        {
            if (config == null) return null;

            var instance = new ItemInstance(config);

            if (config.activeSkill != null)
            {
                instance.SetActiveSkill(new ActiveSkillInstance(config.activeSkill));
            }
            
            var randomConfig = GetRandomPassiveConfigFromPool();
            if (randomConfig != null)
            {
                instance.SetPassiveSkill(new PassiveSkillInstance(randomConfig));
            }

            return instance;
        }

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
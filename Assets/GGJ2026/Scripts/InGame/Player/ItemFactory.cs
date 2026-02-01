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

        [Header("成長設定")]
        [SerializeField, Tooltip("1フロアごとのステータス上昇率 (例: 0.1 = +10%)")]
        private float growthRatePerFloor = 0.1f;
        
        [Header("参照")]
        [SerializeField] private GridView gridView;

        private new void Awake()
        {
            InitializeSingleton();
        }
        
        public void SpawnItem(ItemInstance instance)
        {
            if (gridView != null)
            {
                gridView.SpawnItem(instance, -1, -1);
            }
            else
            {
                Debug.LogError("ItemFactory: GridView の参照が設定されていません！");
            }
        }

        // ★修正: floorを受け取る
        public ItemInstance ChooseItem(int floor)
        {
            if (allItemConfig == null || allItemConfig.Count == 0)
            {
                Debug.LogWarning("ItemFactory: 抽選用のItemConfigリストが空です。");
                return null;
            }
            int index = Random.Range(0, allItemConfig.Count);
            ItemConfig selectedConfig = allItemConfig[index];

            return CreateItem(selectedConfig, floor);
        }

        // ★修正: floorを受け取る
        public ItemInstance CreateItem(ItemConfig config, int floor)
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
                // ★修正: フロアと成長率を渡して数値を計算させる
                instance.SetPassiveSkill(new PassiveSkillInstance(randomConfig, floor, growthRatePerFloor));
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
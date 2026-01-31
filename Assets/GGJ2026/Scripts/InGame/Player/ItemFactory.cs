using System.Collections.Generic;
using System.Linq; // 重み抽選に必要
using GGJ2026.Core.Managers; // Singletonの場所に合わせて変更してください
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
        
        [Header("スポーン設定")]
        [SerializeField] private Transform itemContainer; // アイテムの生成親（Canvas内のItemContainerなど）
        [SerializeField] private GridView gridView;       // DraggableItemの初期化に必要
        [SerializeField] private float minSpawnRadius = 600f; // グリッド中心からの最小距離（グリッド枠外になるように調整）
        [SerializeField] private float maxSpawnRadius = 800f; // グリッド中心からの最大距離

        private new void Awake()
        {
            InitializeSingleton();
        }
        
        public void SpawnItem(ItemInstance instance)
        {
            Transform parent = itemContainer;
            
            GameObject obj = Instantiate(instance.Config.prefab, parent);
            
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minSpawnRadius, maxSpawnRadius);
            
            obj.transform.localPosition = randomDir * distance;
            obj.transform.localScale = Vector3.one;
            
            DraggableItem draggable = obj.GetComponent<DraggableItem>();
            if (draggable != null)
            {
                draggable.Initialize(instance, gridView, -1, -1);
            }
            
            Debug.Log($"Item Spawned Outside: {instance.Config.itemName} at {obj.transform.localPosition}");
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

        /// <summary>
        /// アイテムデータを生成する
        /// </summary>
        public ItemInstance CreateItem(ItemConfig config)
        {
            if (config == null) return null;

            var instance = new ItemInstance(config);

            // 1. アクティブスキルの生成
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
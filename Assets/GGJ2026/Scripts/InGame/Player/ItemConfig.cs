using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026.InGame
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "AddSO/Item")]
    public class ItemConfig : ScriptableObject
    {
        [Header("アイテム名")]
        public string itemName;
        [Header("スプライト")] 
        public Sprite itemSprite;
        [Header("アイテムプレハブ")]
        public GameObject prefab;
        [Header("アイテム形状")]
        public List<Vector2Int> shape = new List<Vector2Int> { new Vector2Int(0, 0) };
        [Header("属性")]
        public ElementType elementType;
        [Header("価値")]
        public int price;
        [Header("アクティブスキル")]
        public ActiveSkillConfig activeSkill; 
        [Header("パッシブスキル")]
        public PassiveSkillConfig passiveSkills;

        public int width = 1;
        public int height = 1;
    }

    public enum ElementType
    {
        Fire,
        Water,
        Earth,
    }
}
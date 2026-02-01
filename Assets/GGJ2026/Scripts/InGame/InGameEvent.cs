using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026.InGame
{
    /// <summary>
    /// ゲーム中のイベント定義
    /// </summary>
    public static class InGameEvent
    {
        /// <summary>
        /// 報酬フェーズ開始イベント
        /// </summary>
        public class OnRewardStartEvent
        {
            public readonly ItemInstance Item1;
            public readonly ItemInstance Item2;
            public readonly ItemInstance Item3;

            public OnRewardStartEvent(ItemInstance item1, ItemInstance item2, ItemInstance item3)
            {
                Item1 = item1;
                Item2 = item2;
                Item3 = item3;
            }
        }

        /// <summary>
        /// 報酬選択イベント
        /// </summary>
        public readonly struct OnRewardSelectedEvent
        {
            public readonly ItemInstance SelectedItem;

            public OnRewardSelectedEvent(ItemInstance selectedItem)
            {
                SelectedItem = selectedItem;
            }
        }
        /// <summary>
        /// メインマスク適用イベント
        /// </summary>
        public class ApplyMainMaskEvent
        {
            public readonly ItemInstance SelectedItem;
            public readonly GameObject SelectedObject;

            public ApplyMainMaskEvent(ItemInstance selectedItem, GameObject selectedObject)
            {
                SelectedItem = selectedItem;
                SelectedObject = selectedObject;
            }
        }

        public class SellMaskEvent
        {
            public readonly ItemInstance SellItem;
            public readonly GameObject SellObject;

            public SellMaskEvent(ItemInstance itemInstance, GameObject selectedObject)
            {
                SellItem = itemInstance;
                SellObject = selectedObject;
            }
        }
        
        /// <summary>
        /// パッシブスキル適用/解除イベント
        /// </summary>
        public readonly struct PassiveEffectEvent
        {
            public readonly PassiveSkillInstance Skill;
            public readonly bool IsEquip; // true:装備(加算), false:解除(減算)

            public PassiveEffectEvent(PassiveSkillInstance skill, bool isEquip)
            {
                Skill = skill;
                IsEquip = isEquip;
            }
        }
    }
}

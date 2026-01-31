using UnityEngine;

namespace GGJ2026.InGame
{
    public enum MaskType
    {
        Yellow,
        Purple,
        Red,
        Blue,
        White
    }

    public enum CharacterAnimState
    {
        Idle,
        Attack
    }

    /// <summary>
    /// マスクのキャラのアニメーションセット
    /// </summary>
    [CreateAssetMenu(menuName = "Player/MaskAnimationSet")]
    public class MaskAnimationSet : ScriptableObject
    {
        public MaskType maskType;

        public Sprite[] idleSprites;   // 3枚固定
        public Sprite[] attackSprites; // 3枚固定

        public float frameInterval = 0.15f;
    }
}
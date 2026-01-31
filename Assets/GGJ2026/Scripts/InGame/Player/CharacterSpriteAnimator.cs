using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026.InGame
{
    /// <summary>
    /// スプライトアニメーションを行う
    /// </summary>
    public class CharacterSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<MaskAnimationSet> maskSets;

        private Dictionary<MaskType, MaskAnimationSet> maskDict;

        private MaskAnimationSet currentMask;
        private CharacterAnimState currentState = CharacterAnimState.Idle;

        private int frameIndex;
        private float timer;

        void Awake()
        {
            maskDict = new Dictionary<MaskType, MaskAnimationSet>();
            foreach (var set in maskSets)
                maskDict[set.maskType] = set;
            ChangeMask(MaskType.Yellow);
        }

        void Update()
        {
            if (currentMask == null) return;

            timer += Time.deltaTime;
            if (timer >= currentMask.frameInterval)
            {
                timer = 0f;
                frameIndex = (frameIndex + 1) % 3;
                spriteRenderer.sprite = GetCurrentSprites()[frameIndex];
            }
        }

        private Sprite[] GetCurrentSprites()
        {
            return currentState == CharacterAnimState.Idle
                ? currentMask.idleSprites
                : currentMask.attackSprites;
        }

        public void ChangeMask(MaskType maskType)
        {
            if (!maskDict.TryGetValue(maskType, out var set)) return;

            currentMask = set;
            ResetAnimation();
        }

        public void PlayIdle()
        {
            currentState = CharacterAnimState.Idle;
            ResetAnimation();
        }

        public void PlayAttack()
        {
            currentState = CharacterAnimState.Attack;
            ResetAnimation();
        }

        private void ResetAnimation()
        {
            frameIndex = 0;
            timer = 0f;
            spriteRenderer.sprite = GetCurrentSprites()[0];
        }
    }
}
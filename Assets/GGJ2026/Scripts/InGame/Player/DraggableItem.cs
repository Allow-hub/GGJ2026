using GGJ2026.Core.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGJ2026.InGame
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableItem : MonoBehaviour, IPointerClickHandler
    {
        public ItemInstance Instance { get; private set; }
        public ItemConfig Config => Instance?.Config;
        
        [HideInInspector] public GridView Controller;
        
        // グリッド上の現在位置
        public int CurrentGridX { get; private set; } = -1;
        public int CurrentGridY { get; private set; } = -1;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Initialize(ItemInstance instance, GridView gridCtrl, int x, int y)
        {
            this.Instance = instance;
            Controller = gridCtrl;
            CurrentGridX = x;
            CurrentGridY = y;
        }

        // クリックされたらGridViewへ通知
        public void OnPointerClick(PointerEventData eventData)
        {
            // ポップアップなどの処理が必要ならここに追加
            // UiManager.I.OpenMaskDescriptionPopup(true, Instance);

            if (Controller != null)
            {
                Controller.OnItemClicked(this);
            }
        }

        // 持ち上げ中はクリック判定を無効化（裏のグリッドをクリックさせるため）
        public void SetRaycastBlock(bool block)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = block;
            }
        }

        public void UpdatePosition(int x, int y, Vector2 localPos)
        {
            CurrentGridX = x;
            CurrentGridY = y;
            rectTransform.anchoredPosition = localPos;
        }
    }
}
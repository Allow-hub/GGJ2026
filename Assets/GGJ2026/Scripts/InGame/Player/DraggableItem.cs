using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGJ2026.InGame
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ItemInstance Instance { get; private set; }
        public ItemConfig Config => Instance?.Config;
        
        [HideInInspector] public GridView Controller;
        
        // グリッド上の現在位置（左下の基準点）
        public int CurrentGridX { get; private set; } = -1;
        public int CurrentGridY { get; private set; } = -1;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private Vector2 originalPos;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // 初期化用メソッド
        public void Initialize(ItemInstance instance, GridView gridCtrl, int x, int y)
        {
            this.Instance = instance;
            Controller = gridCtrl;
            CurrentGridX = x;
            CurrentGridY = y;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;
            originalPos = rectTransform.anchoredPosition;
            
            canvasGroup.blocksRaycasts = false; 
            
            // 【修正】直接 gridSystem を触らず、Controllerのメソッド経由で呼ぶ
            // Controller.gridSystem.Remove(Config); // ← エラー原因
            Controller.OnItemPickedUp(this);      // ← 正解
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            
            // 【修正】メソッド名を GridView 側の定義に合わせる
            // bool success = Controller.TryDropItem(this, eventData.position); // ← エラー原因
            bool success = Controller.OnItemDropped(this, eventData.position);  // ← 正解

            if (!success)
            {
                // 配置失敗：元の位置に戻す
                transform.SetParent(originalParent);
                rectTransform.anchoredPosition = originalPos;
                
                // 【修正】論理データの復帰も Controller のメソッドを使う
                if(CurrentGridX != -1) 
                {
                    // Controller.Logic.Place(...); // ← Logicなんてプロパティはないのでエラー
                    Controller.RevertPlacement(this, CurrentGridX, CurrentGridY); // ← 正解
                }
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
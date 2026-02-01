using GGJ2026.Core.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGJ2026.InGame
{
    public class GridView : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int width = 3;
        [SerializeField] private int height = 3;
        [SerializeField] private float cellSize = 200f; 
        
        [Header("Boundary Settings")]
        [SerializeField] private float backgroundWidth = 1085f;
        [SerializeField] private float backgroundHeight = 950f;

        [Header("References")]
        [SerializeField] private RectTransform gridOrigin; 
        [SerializeField] private RectTransform itemContainer;

        public RectTransform ItemContainer => itemContainer;

        private GridSystem gridSystem; 
        private Canvas rootCanvas;

        private DraggableItem holdingItem = null;
        private float lastPickupTime = 0f;
        private const float PICKUP_COOLDOWN = 0.1f;

        private void Awake()
        {
            gridSystem = new GridSystem(width, height);
            rootCanvas = GetComponentInParent<Canvas>();
        }
        
        private void Start()
        {
            // イベント購読開始
            if (InGameManager.IsValid())
            {
                InGameManager.I.EventBus.Subscribe<InGameEvent.ApplyMainMaskEvent>(OnApplyMainMask);
            }
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (InGameManager.IsValid())
            {
                InGameManager.I.EventBus.Unsubscribe<InGameEvent.ApplyMainMaskEvent>(OnApplyMainMask);
            }
        }

        // ★追加: メインマスク適用イベント受信時の処理
        private void OnApplyMainMask(InGameEvent.ApplyMainMaskEvent e)
        {
            GameObject targetObj = e.SelectedObject;
            
            if (targetObj != null)
            {
                DraggableItem item = targetObj.GetComponent<DraggableItem>();
                if (item != null)
                {
                    // 1. グリッドデータから削除 (もしグリッド上に配置されていたら)
                    if (item.CurrentGridX != -1 && item.CurrentGridY != -1)
                    {
                        gridSystem.RemoveItem(item.Config, item.CurrentGridX, item.CurrentGridY);
                    }

                    // 2. パッシブ効果の解除 (装備されていたらステータスを減算して戻す)
                    // ※ApplyMainMaskの効果自体でステータスが上がるとしても、
                    //   「装備品としての効果」は一度外れるはずなので解除を送る
                    if (item.Instance.PassiveSkill != null)
                    {
                        InGameManager.I.EventBus.Publish(new InGameEvent.PassiveEffectEvent(item.Instance.PassiveSkill, false));
                    }

                    // 3. 持ち上げ中のアイテムだった場合の参照解除
                    if (holdingItem == item)
                    {
                        holdingItem = null;
                    }
                }

                // 4. オブジェクトの破壊
                Destroy(targetObj);
                Debug.Log($"[GridView] Item destroyed via ApplyMainMask: {e.SelectedItem.Config.itemName}");
            }
        }

        private void Update()
        {
            if (holdingItem != null)
            {
                FollowMouse(holdingItem);

                if (Input.GetMouseButtonDown(0) && Time.time > lastPickupTime + PICKUP_COOLDOWN)
                {
                    TryPlaceItem(Input.mousePosition);
                }
            }
        }
        
        private void FollowMouse(DraggableItem item)
        {
            if (item == null) return;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemContainer, 
                Input.mousePosition,
                GetCanvasCamera(),
                out localPoint
            );
            item.transform.localPosition = localPoint;
        }
        
        // --- 生成関連 ---
        public void SpawnItem(ItemInstance instance, int x, int y)
        {
            GameObject obj = Instantiate(instance.Config.prefab, itemContainer);
            obj.transform.localScale = Vector3.one; 
            
            DraggableItem draggable = obj.GetComponent<DraggableItem>();
            draggable.Initialize(instance, this, -1, -1);

            if (x == -1 || y == -1)
            {
                if (!PlaceItemOutside(draggable))
                {
                    float fallbackX = -200f; 
                    float fallbackY = 300f;
                    draggable.UpdatePosition(-1, -1, new Vector2(fallbackX, fallbackY));
                }
            }
            else
            {
                if (!gridSystem.CanPlaceItem(instance.Config, x, y))
                {
                    Debug.LogWarning("初期配置失敗。外側に配置します。");
                    PlaceItemOutside(draggable);
                    return;
                }
                draggable.Initialize(instance, this, x, y);
                gridSystem.PlaceItem(instance.Config, x, y);
                if (instance.PassiveSkill != null)
                    InGameManager.I.EventBus.Publish(new InGameEvent.PassiveEffectEvent(instance.PassiveSkill, true));
                
                draggable.UpdatePosition(x, y, GetLocalPosFromGrid(x, y, instance.Config));
            }
        }

        // --- クリック・配置ロジック ---
        public void OnItemClicked(DraggableItem item)
        {
            if (holdingItem != null) return;

            holdingItem = item;
            lastPickupTime = Time.time; 

            // 詳細ポップアップ表示
            if (UiManager.I != null)
            {
                UiManager.I.OpenMaskDescriptionPopup(true, item.Instance, item.gameObject);
            }

            if (item.CurrentGridX != -1 && item.CurrentGridY != -1)
            {
                gridSystem.RemoveItem(item.Config, item.CurrentGridX, item.CurrentGridY);

                if (item.Instance.PassiveSkill != null)
                {
                    InGameManager.I.EventBus.Publish(new InGameEvent.PassiveEffectEvent(item.Instance.PassiveSkill, false));
                }
            }

            item.SetRaycastBlock(false);
            item.transform.SetAsLastSibling();

            Debug.Log($"Picked up: {item.Config.itemName}");
        }

        private void TryPlaceItem(Vector2 screenPosition)
        {
            if (holdingItem == null) return;

            if (RectTransformUtility.RectangleContainsScreenPoint(itemContainer, screenPosition, GetCanvasCamera()))
            {
                // --- グリッド内 ---
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    itemContainer, 
                    screenPosition, 
                    GetCanvasCamera(), 
                    out localPoint
                );
                
                Vector2 itemSize = holdingItem.GetComponent<RectTransform>().sizeDelta;
                float itemLeftX = localPoint.x - (itemSize.x / 2f);
                float itemBottomY = localPoint.y - (itemSize.y / 2f);

                int x = Mathf.RoundToInt(itemLeftX / cellSize);
                int y = Mathf.RoundToInt(itemBottomY / cellSize);

                if (gridSystem.CanPlaceItem(holdingItem.Config, x, y))
                {
                    gridSystem.PlaceItem(holdingItem.Config, x, y);
                    holdingItem.UpdatePosition(x, y, GetLocalPosFromGrid(x, y, holdingItem.Config));

                    if (holdingItem.Instance.PassiveSkill != null)
                        InGameManager.I.EventBus.Publish(new InGameEvent.PassiveEffectEvent(holdingItem.Instance.PassiveSkill, true));

                    Debug.Log($"[GridView] 配置成功: Grid[{x},{y}]");
                    FinishPlacement();
                }
                else
                {
                    Debug.Log($"[GridView] 配置失敗: Grid[{x},{y}] は埋まっているか、はみ出しています。");
                }
            }
            else
            {
                // --- グリッド外 ---
                Debug.Log("グリッド範囲外をクリック -> 装備を外して配置します");
                if (PlaceItemOutside(holdingItem))
                {
                    FinishPlacement();
                }
                else
                {
                    Debug.LogWarning("外側に置くスペースがありませんでした。");
                }
            }
        }

        private void FinishPlacement()
        {
            if (holdingItem != null)
            {
                if (UiManager.I != null)
                {
                    UiManager.I.OpenMaskDescriptionPopup(false, null);
                }

                holdingItem.SetRaycastBlock(true); 
                holdingItem = null;
            }
        }

        private Camera GetCanvasCamera()
        {
            if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
            return rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main;
        }

        private Vector2 GetLocalPosFromGrid(int x, int y, ItemConfig config)
        {
            float centerX = (x * cellSize) + (config.width * cellSize) / 2f;
            float centerY = (y * cellSize) + (config.height * cellSize) / 2f;
            return new Vector2(centerX, centerY);
        }

        private bool PlaceItemOutside(DraggableItem targetItem)
        {
            float leftSpaceWidth = (backgroundWidth - (width * cellSize)) / 2f; 
            float margin = 10f;
            
            Vector2 itemSize = targetItem.GetComponent<RectTransform>().sizeDelta;

            for (int i = 0; i < 50; i++)
            {
                bool isLeft = Random.Range(0, 2) == 0;
                float x, y;

                if (isLeft)
                {
                    float minX = -leftSpaceWidth + margin;
                    float maxX = 0 - margin;
                    x = Random.Range(minX, maxX);
                }
                else
                {
                    float minX = (width * cellSize) + margin;
                    float maxX = (width * cellSize) + leftSpaceWidth - margin;
                    x = Random.Range(minX, maxX);
                }
                
                float bottomLimit = -(backgroundHeight - (height * cellSize)) / 2f;
                float topLimit = (height * cellSize) + (backgroundHeight - (height * cellSize)) / 2f;
                
                y = Random.Range(bottomLimit + margin, topLimit - margin);

                if (!CheckOverlapWithExistingItems(targetItem, new Vector2(x, y)))
                {
                    targetItem.transform.SetParent(itemContainer);
                    targetItem.UpdatePosition(-1, -1, new Vector2(x, y));
                    return true;
                }
            }
            return false;
        }

        private bool CheckOverlapWithExistingItems(DraggableItem targetItem, Vector2 targetPos)
        {
            Vector2 itemSize = targetItem.GetComponent<RectTransform>().sizeDelta;
            Rect targetRect = new Rect(targetPos.x - itemSize.x / 2f, targetPos.y - itemSize.y / 2f, itemSize.x, itemSize.y);

            foreach (Transform child in itemContainer)
            {
                if (child == targetItem.transform) continue;
                DraggableItem otherItem = child.GetComponent<DraggableItem>();
                if (otherItem != null)
                {
                    RectTransform otherRT = otherItem.GetComponent<RectTransform>();
                    Rect otherRect = new Rect(
                        otherRT.anchoredPosition.x - otherRT.sizeDelta.x / 2f,
                        otherRT.anchoredPosition.y - otherRT.sizeDelta.y / 2f,
                        otherRT.sizeDelta.x,
                        otherRT.sizeDelta.y
                    );
                    if (targetRect.Overlaps(otherRect)) return true; 
                }
            }
            return false;
        }
    }
}
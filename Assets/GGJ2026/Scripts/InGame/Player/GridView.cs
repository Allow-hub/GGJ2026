using GGJ2026.Core.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        [Header("Highlight")]
        [SerializeField] private Color highlightPlaced = new Color(0f, 1f, 0f, 0.3f);

        public RectTransform ItemContainer => itemContainer;

        private GridSystem gridSystem;
        private Canvas rootCanvas;

        private DraggableItem holdingItem = null;
        private float lastPickupTime = 0f;
        private const float PICKUP_COOLDOWN = 0.1f;

        private Image[] cellImages;

        private void Awake()
        {
            gridSystem = new GridSystem(width, height);
            rootCanvas = GetComponentInParent<Canvas>();
            InitializeCells();
        }

        private void InitializeCells()
        {
            cellImages = new Image[width * height];

            for (int i = 0; i < gridOrigin.childCount; i++)
            {
                var img = gridOrigin.GetChild(i).GetComponent<Image>();
                cellImages[i] = img;

                Debug.Log($"CellIndex {i} = {img.name}");
            }
        }

        private void Start()
        {
            if (InGameManager.IsValid())
            {
                InGameManager.I.EventBus.Subscribe<InGameEvent.ApplyMainMaskEvent>(OnApplyMainMask);
                InGameManager.I.EventBus.Subscribe<InGameEvent.SellMaskEvent>(OnSellMask);
            }
        }

        private void OnDestroy()
        {
            if (InGameManager.IsValid())
            {
                InGameManager.I.EventBus.Unsubscribe<InGameEvent.ApplyMainMaskEvent>(OnApplyMainMask);
                InGameManager.I.EventBus.Unsubscribe<InGameEvent.SellMaskEvent>(OnSellMask);
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

            // ★ 常時：配置済みマスを描画
            DrawPlacedItemsHighlight();
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

        // ===== ハイライト（配置済みのみ） =====

        private int GetIndex(int x, int y)
        {
            int invertedY = height - 1 - y;
            return invertedY * width + x;
        }

        private void DrawPlacedItemsHighlight()
        {
            ClearHighlight();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var item = gridSystem.GetItemAt(x, y);
                    if (item != null)
                    {
                        int index = GetIndex(x, y);
                        if (index >= 0 && index < cellImages.Length)
                        {
                            cellImages[index].color = highlightPlaced;
                        }
                    }
                }
            }
        }

        private void ClearHighlight()
        {
            if (cellImages == null) return;
            foreach (var img in cellImages)
            {
                if (img == null) continue;
                img.color = Color.white;
            }
        }

        // ===== アイテム処理 =====

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
                    draggable.UpdatePosition(-1, -1, new Vector2(-200f, 300f));
                }
            }
            else
            {
                if (!gridSystem.CanPlaceItem(instance.Config, x, y))
                {
                    PlaceItemOutside(draggable);
                    return;
                }

                draggable.Initialize(instance, this, x, y);
                gridSystem.PlaceItem(instance.Config, x, y);

                if (instance.PassiveSkill != null)
                {
                    InGameManager.I.EventBus.Publish(
                        new InGameEvent.PassiveEffectEvent(instance.PassiveSkill, true)
                    );
                }

                draggable.UpdatePosition(x, y, GetLocalPosFromGrid(x, y, instance.Config));
            }
        }

        public void OnItemClicked(DraggableItem item)
        {
            if (holdingItem != null) return;

            holdingItem = item;
            lastPickupTime = Time.time;

            // ★ 掴んだ瞬間ハイライト消す
            ClearHighlight();

            if (UiManager.I != null)
                UiManager.I.OpenMaskDescriptionPopup(true, item.Instance, item.gameObject);

            if (item.CurrentGridX != -1 && item.CurrentGridY != -1)
            {
                gridSystem.RemoveItem(item.Config, item.CurrentGridX, item.CurrentGridY);

                if (item.Instance.PassiveSkill != null)
                {
                    InGameManager.I.EventBus.Publish(
                        new InGameEvent.PassiveEffectEvent(item.Instance.PassiveSkill, false)
                    );
                }
            }

            item.SetRaycastBlock(false);
            item.transform.SetAsLastSibling();
        }

        private void TryPlaceItem(Vector2 screenPosition)
        {
            if (holdingItem == null) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemContainer, screenPosition, GetCanvasCamera(), out localPoint
            );

            if (RectTransformUtility.RectangleContainsScreenPoint(itemContainer, screenPosition, GetCanvasCamera()))
            {
                float itemWidthPixels = holdingItem.Config.width * cellSize;
                float itemHeightPixels = holdingItem.Config.height * cellSize;
                float itemLeftX = localPoint.x - (itemWidthPixels / 2f);
                float itemBottomY = localPoint.y - (itemHeightPixels / 2f);

                int x = Mathf.FloorToInt(localPoint.x / cellSize);
                int y = Mathf.FloorToInt(localPoint.y / cellSize);

                // pivotを中央から左下に補正
                x -= holdingItem.Config.width / 2;
                y -= holdingItem.Config.height / 2;

                if (gridSystem.CanPlaceItem(holdingItem.Config, x, y))
                {
                    gridSystem.PlaceItem(holdingItem.Config, x, y);

                    holdingItem.UpdatePosition(x, y, GetLocalPosFromGrid(x, y, holdingItem.Config));

                    if (holdingItem.Instance.PassiveSkill != null)
                    {
                        InGameManager.I.EventBus.Publish(
                            new InGameEvent.PassiveEffectEvent(holdingItem.Instance.PassiveSkill, true)
                        );
                    }

                    FinishPlacement();
                }
            }
            else
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (PlaceItemOutside(holdingItem))
                    {
                        FinishPlacement();
                    }
                }
            }
        }

        private void FinishPlacement()
        {
            if (holdingItem != null)
            {
                if (UiManager.I != null)
                    UiManager.I.OpenMaskDescriptionPopup(false, null);
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

            for (int i = 0; i < 50; i++)
            {
                float x = Random.Range(-leftSpaceWidth + margin, (width * cellSize) + leftSpaceWidth - margin);

                float bottomLimit = -(backgroundHeight - (height * cellSize)) / 2f;
                float topLimit = (height * cellSize) + (backgroundHeight - (height * cellSize)) / 2f;

                float y = Random.Range(bottomLimit + margin, topLimit - margin);

                if (!CheckOverlapWithExistingItems(targetItem, new Vector2(x, y)))
                {
                    targetItem.UpdatePosition(-1, -1, new Vector2(x, y));
                    return true;
                }
            }
            return false;
        }

        private bool CheckOverlapWithExistingItems(DraggableItem targetItem, Vector2 targetPos)
        {
            Vector2 itemSize = targetItem.GetComponent<RectTransform>().sizeDelta;
            Rect targetRect = new Rect(
                targetPos.x - itemSize.x / 2f,
                targetPos.y - itemSize.y / 2f,
                itemSize.x,
                itemSize.y
            );

            foreach (Transform child in itemContainer)
            {
                if (child == targetItem.transform) continue;

                RectTransform otherRT = child.GetComponent<RectTransform>();
                Rect otherRect = new Rect(
                    otherRT.anchoredPosition.x - otherRT.sizeDelta.x / 2f,
                    otherRT.anchoredPosition.y - otherRT.sizeDelta.y / 2f,
                    otherRT.sizeDelta.x,
                    otherRT.sizeDelta.y
                );

                if (targetRect.Overlaps(otherRect)) return true;
            }
            return false;
        }

        // ===== イベント =====

        private void OnApplyMainMask(InGameEvent.ApplyMainMaskEvent e)
        {
            RemoveAndDestroyItem(e.SelectedObject);
        }

        private void OnSellMask(InGameEvent.SellMaskEvent e)
        {
            RemoveAndDestroyItem(e.SellObject);
        }

        private void RemoveAndDestroyItem(GameObject targetObj)
        {
            if (targetObj == null) return;

            DraggableItem item = targetObj.GetComponent<DraggableItem>();

            if (item != null)
            {
                if (holdingItem == item)
                {
                    holdingItem = null;
                }
                else
                {
                    if (item.CurrentGridX != -1 && item.CurrentGridY != -1)
                    {
                        gridSystem.RemoveItem(item.Config, item.CurrentGridX, item.CurrentGridY);
                    }

                    if (item.Instance.PassiveSkill != null)
                    {
                        InGameManager.I.EventBus.Publish(
                            new InGameEvent.PassiveEffectEvent(item.Instance.PassiveSkill, false)
                        );
                    }
                }
            }

            Destroy(targetObj);
        }
    }
}
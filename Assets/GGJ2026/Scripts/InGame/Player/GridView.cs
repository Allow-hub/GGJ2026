using UnityEngine;

namespace GGJ2026.InGame
{
    public class GridView : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int width = 3;
        [SerializeField] private int height = 3;
        [SerializeField] private float cellSize = 200f; 

        [Header("References")]
        [SerializeField] private RectTransform gridOrigin; 
        [SerializeField] private RectTransform itemContainer;
        public RectTransform ItemContainer => itemContainer;
        

        private GridSystem gridSystem; 
        private Canvas rootCanvas;

        private void Awake()
        {
            gridSystem = new GridSystem(width, height);
            rootCanvas = GetComponentInParent<Canvas>();
        }
        
        private void Start()
        {
            SpawnDebugItem();
        }
        private void SpawnDebugItem()
        {
            ItemInstance instance = ItemFactory.I.ChooseItem();

            // ランダムな位置を最大10回試行して、置ける場所を探す
            bool placed = false;
            for (int tryCount = 0; tryCount < 10; tryCount++)
            {
                int x = Random.Range(0, width);
                int y = Random.Range(0, height);

                // Configを使ってサイズ判定
                if (gridSystem.CanPlaceItem(instance.Config, x, y))
                {
                    // ★修正: 生成済みの instance を渡す
                    SpawnItem(instance.Config, x, y);
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"[Debug] {instance.Config.itemName} を配置できる隙間が見つかりませんでした。");
            }
        }
        

        public void SpawnItem(ItemConfig config, int x, int y)
        {
            if (!gridSystem.CanPlaceItem(config, x, y))
            {
                Debug.LogWarning("初期配置に失敗：場所が空いていません");
                return;
            }
            ItemInstance itemInstance = ItemFactory.I.CreateItem(config);

            GameObject obj = Instantiate(config.prefab, itemContainer);
            DraggableItem draggable = obj.GetComponent<DraggableItem>();

            draggable.Initialize(itemInstance, this, x, y);

            gridSystem.PlaceItem(config, x, y);
            
            Vector2 pos = GetLocalPosFromGrid(x, y, config); 
            draggable.UpdatePosition(x, y, pos);
            
            obj.transform.localScale = Vector3.one;
        }

        public void OnItemPickedUp(DraggableItem draggable)
        {
            gridSystem.RemoveItem(draggable.Config, draggable.CurrentGridX, draggable.CurrentGridY);
            if (draggable.Instance.PassiveSkill != null)
            {
                PlayerController.I.ApplyPassiveEffect(draggable.Instance.PassiveSkill, isEquip: false);
            }
        }

        public bool OnItemDropped(DraggableItem draggable, Vector2 screenPosition)
        {
            Camera cam = null;
            if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = rootCanvas.worldCamera;
                if (cam == null) cam = Camera.main; 
            }

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridOrigin, 
                screenPosition, 
                cam, 
                out localPoint
            );

            int x = Mathf.FloorToInt(localPoint.x / cellSize);
            int y = Mathf.FloorToInt(localPoint.y / cellSize);
            
            Debug.Log($"マウス位置: {localPoint}, 計算されたマス: [{x}, {y}]");

            if (gridSystem.CanPlaceItem(draggable.Config, x, y))
            {
                gridSystem.PlaceItem(draggable.Config, x, y);
                SnapItemToGrid(draggable, x, y);
                if (draggable.Instance.PassiveSkill != null)
                {
                    PlayerController.I.ApplyPassiveEffect(draggable.Instance.PassiveSkill, isEquip: true);
                }
                Debug.Log($"Item Placed at [{x},{y}]");
                return true; 
            }
            else
            {
                Debug.Log("Cannot place item here.");
                return false; 
            }
        }

        public void RevertPlacement(DraggableItem draggable, int originalX, int originalY)
        {
             gridSystem.PlaceItem(draggable.Config, originalX, originalY);
        }

        private void SnapItemToGrid(DraggableItem draggable, int x, int y)
        {
            draggable.transform.SetParent(itemContainer);
            
            Vector2 snapPos = GetLocalPosFromGrid(x, y, draggable.Config);
            
            draggable.UpdatePosition(x, y, snapPos); 
            draggable.transform.localScale = Vector3.one; 
        }

        private Vector2 GetLocalPosFromGrid(int x, int y, ItemConfig config)
        {
            float widthOffset = (config.width * cellSize) / 2f;
            float heightOffset = (config.height * cellSize) / 2f;
            
            return new Vector2(
                x * cellSize + widthOffset, 
                y * cellSize + heightOffset
            );
        }
    }
}
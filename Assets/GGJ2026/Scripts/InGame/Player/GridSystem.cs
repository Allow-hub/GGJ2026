using UnityEngine;

namespace GGJ2026.InGame
{
    public class GridSystem
    {
        private int width;
        private int height;
        
        private ItemConfig[,] gridCells;

        public GridSystem(int w, int h)
        {
            width = w;
            height = h;
            gridCells = new ItemConfig[width, height];
        }
        
        public bool CanPlaceItem(ItemConfig item, int pivotX, int pivotY)
        {
            foreach (var offset in item.shape)
            {
                int targetX = pivotX + offset.x;
                int targetY = pivotY + offset.y;

                // グリッドの範囲外チェック
                if (targetX < 0 || targetX >= width || targetY < 0 || targetY >= height)
                {
                    return false;
                }

                // 既に他のアイテムがあるかチェック
                if (gridCells[targetX, targetY] != null)
                {
                    return false;
                }
            }

            return true;
        }
        
        public void PlaceItem(ItemConfig item, int pivotX, int pivotY)
        {
            if (!CanPlaceItem(item, pivotX, pivotY)) return;

            foreach (var offset in item.shape)
            {
                int x = pivotX + offset.x;
                int y = pivotY + offset.y;
                gridCells[x, y] = item;
            }
        }
        
        public void RemoveItem(ItemConfig item, int pivotX, int pivotY)
        {
            foreach (var offset in item.shape)
            {
                int x = pivotX + offset.x;
                int y = pivotY + offset.y;
                
                // 範囲内かつ、そのアイテムがあれば削除
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    if (gridCells[x, y] == item)
                    {
                        gridCells[x, y] = null;
                    }
                }
            }
        }
        
        //Debug
        public ItemConfig GetItemAt(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return null;
            return gridCells[x, y];
        }
    }
}
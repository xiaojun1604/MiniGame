using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Data;

namespace Game.UI
{
    public class GridSystem : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RectTransform container;
        [SerializeField] private GridLayoutGroup gridLayout;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject cellPrefab; // We will generate this if null
        
        private Dictionary<Vector2Int, GridCellView> _cells = new Dictionary<Vector2Int, GridCellView>();
        
        public float CellSize { get; private set; }

        public void GenerateGrid(LevelData level)
        {
            if (container == null) container = GetComponent<RectTransform>();
            if (gridLayout == null) gridLayout = GetComponent<GridLayoutGroup>();

            ClearGrid();
            
            // 1. Calculate Cell Size
            float width = container.rect.width;
            float height = container.rect.height;
            float padding = 10f;
            float spacing = 5f;
            
            gridLayout.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            gridLayout.spacing = new Vector2(spacing, spacing);
            
            float availableW = width - (padding * 2) - (spacing * (level.cols - 1));
            float availableH = height - (padding * 2) - (spacing * (level.rows - 1));
            
            float sizeW = availableW / level.cols;
            float sizeH = availableH / level.rows;
            
            CellSize = Mathf.Min(sizeW, sizeH);
            
            gridLayout.cellSize = new Vector2(CellSize, CellSize);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = level.cols;
            
            // 2. Spawn Cells
            // Grid layout order is Top-Left to Bottom-Right typically, or depends on StartCorner.
            // We assume standard (Upper Left). relative to Row 0 ??
            // Let's rely on standard creation order interaction with standard loops.
            
            for (int r = 0; r < level.rows; r++)
            {
                for (int c = 0; c < level.cols; c++)
                {
                    Vector2Int coord = new Vector2Int(c, r); // x=col, y=row. Note: Y usually goes UP in connection logic, but Grid flows DOWN.
                    // We need to map visual index to logic coordinate.
                    // Let's assume (0,0) is Top-Left for visual simplicity in Grid,
                    // BUT for game logic, usually (0,0) is Bottom-Left. 
                    // Let's stick to (c, r) where r is row index from top (0) to bottom (rows-1).
                    
                    CreateCell(coord, level);
                }
            }
        }

        private void CreateCell(Vector2Int coord, LevelData level)
        {
            GameObject cellObj = null;
            if (cellPrefab != null)
            {
                cellObj = Instantiate(cellPrefab, container);
            }
            else
            {
                // Fallback: Generate Default Cell Object Code-wise
                cellObj = new GameObject($"Cell_{coord.x}_{coord.y}", typeof(Image));
                cellObj.transform.SetParent(container, false);
                var img = cellObj.GetComponent<Image>();
                img.sprite = null; // White block
            }
            
            // Add View Component
            var view = cellObj.GetComponent<GridCellView>();
            if (view == null) view = cellObj.AddComponent<GridCellView>();
            
            // Determine Type
            CellType type = CellType.Empty;
            if (level.targetPositions.Contains(coord)) type = CellType.Target;
            if (level.obstaclePositions.Contains(coord)) type = CellType.Obstacle;
            if (level.targetPositions.Count > 0 && level.targetPositions[0] == coord) type = CellType.Start;

            view.Init(coord, type, CellSize);
            _cells[coord] = view;
        }

        private void ClearGrid()
        {
            if (container != null)
            {
                foreach (Transform child in container)
                {
                    Destroy(child.gameObject);
                }
            }
            _cells.Clear();
        }
        
        public GridCellView GetCell(Vector2Int coord)
        {
            if (_cells.TryGetValue(coord, out var cell)) return cell;
            return null;
        }

        public GridCellView GetCellAtWorldPosition(Vector2 worldPos)
        {
            // Simple Raycast using RectTransform
            // Since we don't want to loop all, we might rely on Unity EventSystem or just Loop Closest.
            // For N=100, looping is fine.
            foreach (var kvp in _cells)
            {
                RectTransform rt = kvp.Value.GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(rt, worldPos))
                {
                    return kvp.Value;
                }
            }
            return null;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Game.Data;
using Game.UI;

namespace Game.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("References")]
        public GridSystem gridSystem;
        public RectTransform lineContainer; // Should be above Grid in hierarchy
        
        [Header("Settings")]
        public LevelData currentLevel;
        public float lineThickness = 10f;
        public Color lineColor = Color.cyan;

        private List<GridCellView> currentPath = new List<GridCellView>();
        private List<LineConnector> currentLines = new List<LineConnector>();
        private bool isDragging = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // If no level assigned, create debug
            if (currentLevel == null)
            {
                currentLevel = LevelData.CreateDebugLevel();
            }
            StartLevel(currentLevel);
        }

        public void StartLevel(LevelData level)
        {
            if (gridSystem == null)
            {
                gridSystem = FindObjectOfType<GridSystem>();
            }

            if (gridSystem == null)
            {
                Debug.LogError("GridSystem not found in scene!");
                return;
            }

            currentLevel = level;
            currentPath.Clear();
            ClearLines();
            gridSystem.GenerateGrid(level);
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnPointerDown();
            }
            else if (Input.GetMouseButton(0))
            {
                OnPointerDrag();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnPointerUp();
            }
        }

        private void OnPointerDown()
        {
            GridCellView cell = GetCellUnderMouse();
            if (cell != null && cell.Type == CellType.Target)
            {
                // Only start if it's a valid start point (Visual aid? Or any Target?)
                // Allow starting from ANY target for now, or specifically Start type?
                // Doc says "Start from Pi Dan".
                StartPath(cell);
            }
        }

        private void OnPointerDrag()
        {
            if (!isDragging) return;

            GridCellView cell = GetCellUnderMouse();
            if (cell != null)
            {
                TryAddToPath(cell);
            }
        }

        private void OnPointerUp()
        {
            if (!isDragging) return;
            isDragging = false;
            CheckWin();
        }

        private void StartPath(GridCellView startCell)
        {
            isDragging = true;
            // Clear old path if we start fresh? Or continue? Usually fresh.
            ResetPathVisuals();
            currentPath.Clear();
            ClearLines();
            
            AddCellToPath(cell: startCell);
        }
        
        private void TryAddToPath(GridCellView cell)
        {
            if (currentPath.Contains(cell))
            {
                // Backtrack check
                if (currentPath.Count >= 2 && cell == currentPath[currentPath.Count - 2])
                {
                    // Convert Back
                    RemoveLastStep();
                }
                return;
            }

            // Check if neighbor
            GridCellView lastCell = currentPath[currentPath.Count - 1];
            if (IsNeighbor(lastCell.Coordinate, cell.Coordinate))
            {
                // Check if obstacle
                if (cell.Type == CellType.Obstacle) return;

                AddCellToPath(cell);
            }
        }

        private bool IsNeighbor(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            
            if (currentLevel.allowDiagonal)
            {
                return Mathf.Max(dx, dy) == 1 && !(dx==0 && dy==0);
            }
            else
            {
                return (dx + dy) == 1;
            }
        }

        private void AddCellToPath(GridCellView cell)
        {
            currentPath.Add(cell);
            cell.SetVisited(true);
            
            // Draw Line from previous
            if (currentPath.Count > 1)
            {
                GridCellView prev = currentPath[currentPath.Count - 2];
                CreateLine(prev.transform.position, cell.transform.position);
            }
        }

        private void RemoveLastStep()
        {
            if (currentPath.Count <= 1) return;
            
            GridCellView last = currentPath[currentPath.Count - 1];
            last.SetVisited(false);
            currentPath.RemoveAt(currentPath.Count - 1);
            
            // Remove Line
            if (currentLines.Count > 0)
            {
                LineConnector line = currentLines[currentLines.Count - 1];
                Destroy(line.gameObject);
                currentLines.RemoveAt(currentLines.Count - 1);
            }
        }

        private void CreateLine(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject("Line", typeof(RectTransform));
            lineObj.transform.SetParent(lineContainer, false);
            
            LineConnector line = lineObj.AddComponent<LineConnector>();
            line.Setup(start, end, lineThickness, lineColor);
            currentLines.Add(line);
        }

        private void ClearLines()
        {
            foreach (var line in currentLines)
            {
                if(line != null) Destroy(line.gameObject);
            }
            currentLines.Clear();
        }

        private void ResetPathVisuals()
        {
            foreach(var c in currentPath) c.SetVisited(false);
        }

        private void CheckWin()
        {
            // Count targets
            int required = currentLevel.targetPositions.Count;
            // Or simpler: Did we visit ALL targets?
            
            int visitedTargets = 0;
            foreach(var node in currentPath)
            {
                if (node.Type == CellType.Target || node.Type == CellType.Start) visitedTargets++;
            }
            
            // Note: Since Start is in targetPositions usually
            if (visitedTargets == required)
            {
                 Debug.Log("WIN!");
                 // Show Win UI
            }
            else
            {
                Debug.Log("Incomplete. Reset.");
                ResetPathVisuals();
                currentPath.Clear();
                ClearLines();
            }
        }

        private GridCellView GetCellUnderMouse()
        {
            return gridSystem.GetCellAtWorldPosition(Input.mousePosition);
        }
    }
}

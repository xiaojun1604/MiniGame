using UnityEngine;
using UnityEngine.UI;
using Game.Data;

namespace Game.UI
{
    public class GridCellView : MonoBehaviour
    {
        public Vector2Int Coordinate { get; private set; }
        public CellType Type { get; private set; }
        
        [SerializeField] private Image bgImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text debugText; // Optional

        private Color colorTarget = new Color(1f, 0.8f, 0.2f); // Orange
        private Color colorObstacle = new Color(0.3f, 0.3f, 0.3f); // Dark Gray
        private Color colorVisited = new Color(0.2f, 1f, 0.5f); // Green
        private Color colorDefault = Color.white;

        public void Init(Vector2Int coord, CellType type, float size)
        {
            this.Coordinate = coord;
            this.Type = type;
            
            // Adjust size
            RectTransform rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);
            
            // Visuals
            UpdateVisuals(false);

            if(debugText != null) debugText.text = $"{coord.x},{coord.y}";
        }

        public void SetVisited(bool visited)
        {
            if (Type == CellType.Obstacle) return;
            
            if (visited)
            {
               bgImage.color = colorVisited;
            }
            else
            {
               UpdateVisuals(false); 
            }
        }

        private void UpdateVisuals(bool visited)
        {
            if (bgImage == null) bgImage = GetComponent<Image>();
            
            switch (Type)
            {
                case CellType.Target:
                    bgImage.color = colorTarget;
                    break;
                case CellType.Obstacle:
                    bgImage.color = colorObstacle;
                    break;
                case CellType.Start:
                    bgImage.color = Color.cyan;
                    break;
                default:
                    bgImage.color = colorDefault;
                    break;
            }
        }
    }
}

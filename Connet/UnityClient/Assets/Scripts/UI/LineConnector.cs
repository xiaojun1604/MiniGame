using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    // A single segment of the line
    public class LineConnector : MonoBehaviour
    {
        [SerializeField] private Image lineImage;

        public void Setup(Vector3 startPos, Vector3 endPos, float thickness, Color color)
        {
            if (lineImage == null)
            {
                lineImage = gameObject.AddComponent<Image>();
            }
            lineImage.color = color;

            UpdatePosition(startPos, endPos, thickness);
        }

        public void UpdatePosition(Vector3 startPos, Vector3 endPos, float thickness)
        {
            RectTransform rt = GetComponent<RectTransform>();
            rt.pivot = new Vector2(0, 0.5f); // Pivot at start, center Y
            rt.position = startPos;
            
            Vector3 dir = endPos - startPos;
            float dist = dir.magnitude;
            
            rt.sizeDelta = new Vector2(dist, thickness);
            
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rt.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}

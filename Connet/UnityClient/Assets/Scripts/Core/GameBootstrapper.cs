using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.UI;

namespace Game.Core
{
    public class GameBootstrapper : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void InitGame()
        {
            // Check if GameManager exists
            if (FindObjectOfType<GameManager>() != null) return;

            Debug.Log("Bootstrapping Game...");

            // 1. Create EventSystem
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            // 2. Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 3. Create Background
            GameObject bgObj = new GameObject("Background", typeof(Image));
            bgObj.transform.SetParent(canvasObj.transform, false);
            bgObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            bgObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bgObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            
            // Try load background sprite
            Sprite bgSprite = Resources.Load<Sprite>("bg_game"); // If in Resources? 
            // We put it in Assets/Sprites. Resources.Load only works if in Assets/Resources.
            // Since we can't move it easily via code at runtime without Editor, we'll try to load or just set color.
            bgObj.GetComponent<Image>().color = new Color(0.9f, 0.95f, 1f);

            // 4. Create GameContainer (Centered Square)
            GameObject container = new GameObject("GameContainer", typeof(RectTransform));
            container.transform.SetParent(canvasObj.transform, false);
            RectTransform containerRT = container.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0.5f);
            containerRT.anchorMax = new Vector2(0.5f, 0.5f);
            containerRT.sizeDelta = new Vector2(800, 800); // Fixed size for now
            container.AddComponent<GridLayoutGroup>();

            // 5. Create LineContainer (Above Grid)
            GameObject lineContainer = new GameObject("LineContainer", typeof(RectTransform));
            lineContainer.transform.SetParent(canvasObj.transform, false);
            RectTransform linesRT = lineContainer.GetComponent<RectTransform>();
            linesRT.anchorMin = new Vector2(0.5f, 0.5f);
            linesRT.anchorMax = new Vector2(0.5f, 0.5f);
            linesRT.sizeDelta = new Vector2(800, 800); // Match container

            // 6. Setup Managers
            GameObject mgrObj = new GameObject("GameManager");
            GameManager mgr = mgrObj.AddComponent<GameManager>();
            GridSystem grid = container.AddComponent<GridSystem>(); // GridSystem sits on Container usually for ease

            mgr.gridSystem = grid;
            mgr.lineContainer = linesRT;
            
            Debug.Log("Game Bootstrapped. Ready to play.");
        }
    }
}

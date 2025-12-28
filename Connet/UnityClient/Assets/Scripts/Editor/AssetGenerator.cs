#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Game.Editor
{
    public class AssetGenerator : MonoBehaviour
    {
        [MenuItem("OneLineLink/Generate Missing Sprites")]
        public static void GenerateSprites()
        {
            string folder = "Assets/Sprites";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            GenerateCircle(folder + "/line_brush.png", 64, Color.white, 0f);
            GenerateRoundedRect(folder + "/cell_base.png", 128, 20, new Color(1f, 1f, 1f, 0.5f)); // Semi-transparent white
            GenerateIcon(folder + "/cell_target.png", 128, Color.yellow, "circle");
            GenerateIcon(folder + "/cell_obstacle.png", 128, Color.gray, "cross");

            AssetDatabase.Refresh();
            Debug.Log("Sprites Generated in Assets/Sprites/");
        }

        static void GenerateCircle(string path, int size, Color color, float blur)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            Color[] colors = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = 0;
                    if (dist < radius) alpha = 1;
                    if (dist > radius - 1 && dist < radius + 1) alpha = 0.5f; // Antialias simple
                    if (dist >= radius) alpha = 0;
                    
                    colors[y * size + x] = new Color(color.r, color.g, color.b, alpha * color.a);
                }
            }
            
            tex.SetPixels(colors);
            SaveTex(tex, path);
        }

        static void GenerateRoundedRect(string path, int size, int radius, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            Color[] colors = new Color[size * size];
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Simple Box SDF or just fill
                    // A simple approximation: if closest distance to edge > radius...
                    // Let's just do a simple block for now for robustness, or a standard math for rounded box.
                    // Math: q = abs(p) - b + r. dist = length(max(q,0)) + min(max(q.x,q.y),0) - r
                    
                    // Simplified: Draw a solid box
                    // Ideally we usually check if x,y is within the rounded corners.
                    
                    bool inside = true; // Make it simple: Box
                    if ((x < radius && y < radius && Vector2.Distance(new Vector2(x,y), new Vector2(radius, radius)) > radius) ||
                        (x > size-radius && y < radius && Vector2.Distance(new Vector2(x,y), new Vector2(size-radius, radius)) > radius) ||
                        (x < radius && y > size-radius && Vector2.Distance(new Vector2(x,y), new Vector2(radius, size-radius)) > radius) ||
                        (x > size-radius && y > size-radius && Vector2.Distance(new Vector2(x,y), new Vector2(size-radius, size-radius)) > radius))
                    {
                        inside = false;
                    }

                    colors[y * size + x] = inside ? color : Color.clear;
                }
            }
            tex.SetPixels(colors);
            SaveTex(tex, path);
        }

        static void GenerateIcon(string path, int size, Color color, string shape)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            Color[] pix = new Color[size*size];
            Vector2 center = new Vector2(size/2f, size/2f);
            float r = size * 0.4f;

            for(int i=0; i<pix.Length; i++) pix[i] = Color.clear;
            tex.SetPixels(pix);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool draw = false;
                    if (shape == "circle")
                    {
                        if (Vector2.Distance(new Vector2(x,y), center) < r) draw = true;
                    }
                    else if (shape == "cross")
                    {
                        // Draw X
                        if (Mathf.Abs(x - y) < 8 || Mathf.Abs(x - (size - y)) < 8) draw = true;
                    }
                    
                    if (draw) tex.SetPixel(x, y, color);
                }
            }
            SaveTex(tex, path);
        }

        static void SaveTex(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }
    }
}
#endif

using UnityEngine;
using System.Collections.Generic;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "NewLevel", menuName = "OneLineLink/Level Data")]
    public class LevelData : ScriptableObject
    {
        public int rows = 5;
        public int cols = 5;
        public bool allowDiagonal = false;
        
        [Tooltip("List of coordinates that are Targets (Pi Dan)")]
        public List<Vector2Int> targetPositions = new List<Vector2Int>();
        
        [Tooltip("List of coordinates that are Obstacles")]
        public List<Vector2Int> obstaclePositions = new List<Vector2Int>();

        // Helper to create a level from code (for testing/bootstrapping)
        public static LevelData CreateDebugLevel()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            level.rows = 4;
            level.cols = 4;
            level.allowDiagonal = true;
            
            // Create a simple path: (0,0) -> (0,1) -> (0,2) -> (1,2)
            level.targetPositions.Add(new Vector2Int(0, 0));
            level.targetPositions.Add(new Vector2Int(0, 1));
            level.targetPositions.Add(new Vector2Int(0, 2));
            level.targetPositions.Add(new Vector2Int(1, 2));
            level.targetPositions.Add(new Vector2Int(1, 1)); // End
            
            // Add an obstacle
            level.obstaclePositions.Add(new Vector2Int(2, 2));
            
            return level;
        }
    }
}

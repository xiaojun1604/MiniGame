using UnityEngine;
using System.Collections.Generic;

namespace Game.Data
{
    public enum CellType
    {
        Empty = 0,
        Target = 1,   // The "Pi Dan" to collect
        Obstacle = 2, // The "Normal Egg" to avoid
        Start = 3     // Starting point (is also a Target)
    }
}

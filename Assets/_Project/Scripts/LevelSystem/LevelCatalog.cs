using System.Collections.Generic;
using UnityEngine;

namespace MasterParking.LevelSystem
{
    /// <summary>
    /// Ordered list of levels. Adding a level is an asset edit, not a code edit.
    /// </summary>
    [CreateAssetMenu(menuName = "Master Parking/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        public List<LevelData> Levels = new List<LevelData>();
    }
}

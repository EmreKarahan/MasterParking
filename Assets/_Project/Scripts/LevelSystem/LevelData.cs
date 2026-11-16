using MasterParking.Core;
using UnityEngine;

namespace MasterParking.LevelSystem
{
    /// <summary>
    /// One authored level. The importer creates these assets from the migrated
    /// JSON, which keeps designers in the inspector and gameplay code generic.
    /// </summary>
    [CreateAssetMenu(menuName = "Master Parking/Level", fileName = "Level_")]
    public sealed class LevelData : ScriptableObject
    {
        public LevelDefinition Definition = new LevelDefinition();

        public int Id => Definition != null ? Definition.Id : 0;
        public string DisplayName => Definition != null && !string.IsNullOrEmpty(Definition.Name)
            ? Definition.Name
            : name;
    }
}

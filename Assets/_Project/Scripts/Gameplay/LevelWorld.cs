using System.Collections.Generic;
using MasterParking.Core;
using UnityEngine;

namespace MasterParking.Gameplay
{
    /// <summary>The built scene graph plus the systems that need per-frame updates.</summary>
    public sealed class LevelWorld
    {
        public GameObject Root;
        public LevelDefinition Level;
        public LevelBounds Bounds;
        public ObstacleRegistry Obstacles;
        public List<TrafficController> Traffic = new List<TrafficController>();
        public Transform TargetMarker;

        public void Dispose()
        {
            Traffic.Clear();
            Obstacles?.Clear();
            if (Root != null)
            {
                Object.Destroy(Root);
                Root = null;
            }
        }
    }
}

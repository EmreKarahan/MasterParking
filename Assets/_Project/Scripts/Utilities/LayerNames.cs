using UnityEngine;

namespace MasterParking.Utilities
{
    /// <summary>
    /// Optional custom layers are created by the editor setup tool. Gameplay keeps
    /// working with the default layer so a fresh checkout can be played immediately.
    /// </summary>
    public static class LayerNames
    {
        public const string Vehicle = "Vehicle";
        public const string Obstacle = "Obstacle";

        public static int Resolve(string layerName, int fallback = 0)
        {
            int layer = LayerMask.NameToLayer(layerName);
            return layer >= 0 ? layer : fallback;
        }

        public static int ObstacleLayer => Resolve(Obstacle);

        public static int ObstacleMask => 1 << ObstacleLayer;

        public static void SetLayerRecursively(GameObject root, int layer)
        {
            if (root == null)
            {
                return;
            }

            root.layer = layer;
            Transform transform = root.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                SetLayerRecursively(transform.GetChild(i).gameObject, layer);
            }
        }
    }
}

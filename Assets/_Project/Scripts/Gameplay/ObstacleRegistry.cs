using System.Collections.Generic;
using UnityEngine;

namespace MasterParking.Gameplay
{
    /// <summary>
    /// Maps level colliders to gameplay meaning. The car's overlap query only
    /// reports colliders in this registry, so decorative or ground colliders can
    /// never cause a false crash.
    /// </summary>
    public sealed class ObstacleRegistry
    {
        private readonly Dictionary<Collider, ObstacleType> _colliders =
            new Dictionary<Collider, ObstacleType>(64);

        public int Count => _colliders.Count;

        public void Register(Collider collider, ObstacleType type)
        {
            if (collider != null)
            {
                _colliders[collider] = type;
            }
        }

        public bool TryGet(Collider collider, out ObstacleType type)
        {
            if (collider != null && _colliders.TryGetValue(collider, out type))
            {
                return true;
            }

            type = default;
            return false;
        }

        public void Clear() => _colliders.Clear();
    }
}

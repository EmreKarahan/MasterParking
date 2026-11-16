using UnityEngine;

namespace MasterParking.Gameplay
{
    /// <summary>
    /// Thin overlap query around the car body. Only registered obstacles count,
    /// which keeps the ground and decorative props out of the crash pipeline.
    /// </summary>
    public sealed class VehicleCollisionProbe
    {
        private static readonly Vector3 HalfExtents = new Vector3(3.5f, 1.45f, 1.55f);
        private static readonly Vector3 CenterOffset = new Vector3(0f, 1.45f, 0f);

        private readonly Collider[] _hits = new Collider[24];
        private readonly ObstacleRegistry _registry;

        public VehicleCollisionProbe(ObstacleRegistry registry)
        {
            _registry = registry;
        }

        public bool TryDetect(Vector3 position, Quaternion rotation, out ObstacleType type)
        {
            int count = Physics.OverlapBoxNonAlloc(
                position + CenterOffset,
                HalfExtents,
                _hits,
                rotation,
                ~0,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                if (_registry.TryGet(_hits[i], out type))
                {
                    return true;
                }
            }

            type = default;
            return false;
        }
    }
}

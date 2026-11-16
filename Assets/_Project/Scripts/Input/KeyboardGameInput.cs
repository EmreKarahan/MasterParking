using MasterParking.Core;
using UnityEngine;

namespace MasterParking.Input
{
    /// <summary>
    /// Developer keyboard controls. Compiled only for the editor and development
    /// builds; shipping mobile builds are touch-only.
    /// </summary>
    public sealed class KeyboardGameInput
    {
        public struct Snapshot
        {
            public bool Throttle;
            public bool Brake;
            public bool SteeringHeld;
            public float SteeringTarget;
            public bool ToggleCamera;
            public bool Pause;
            public bool Restart;
            public bool HasGearRequest;
            public Gear GearRequest;
        }

        public Snapshot Poll()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var snapshot = new Snapshot
            {
                Throttle = UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow),
                Brake = UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow) ||
                        UnityEngine.Input.GetKey(KeyCode.Space),
                ToggleCamera = UnityEngine.Input.GetKeyDown(KeyCode.C),
                Pause = UnityEngine.Input.GetKeyDown(KeyCode.Escape),
                Restart = UnityEngine.Input.GetKeyDown(KeyCode.F2)
            };

            bool left = UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow);
            bool right = UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow);
            snapshot.SteeringHeld = left || right;
            snapshot.SteeringTarget = left ? -1f : right ? 1f : 0f;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                snapshot.HasGearRequest = true;
                snapshot.GearRequest = Gear.Park;
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                snapshot.HasGearRequest = true;
                snapshot.GearRequest = Gear.Reverse;
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                snapshot.HasGearRequest = true;
                snapshot.GearRequest = Gear.Drive;
            }

            return snapshot;
#else
            return default;
#endif
        }
    }
}

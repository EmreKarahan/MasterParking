using MasterParking.Core;
using MasterParking.Utilities;
using UnityEngine;

namespace MasterParking.Vehicle
{
    /// <summary>
    /// Mirrors the simulated pose onto the visual hierarchy. The simulation never
    /// touches transforms, and this view never changes simulation state.
    /// </summary>
    public sealed class VehicleView : MonoBehaviour
    {
        private Transform[] _wheels;
        private bool[] _frontWheels;
        private float _wheelRadius = 0.7f;
        private float _wheelSpin;
        private Vector3 _lastPosition;
        private bool _hasLastPosition;

        public void Configure(Transform[] wheels, bool[] frontWheels, float wheelRadius)
        {
            _wheels = wheels;
            _frontWheels = frontWheels;
            _wheelRadius = Mathf.Max(0.1f, wheelRadius);
            _wheelSpin = 0f;
            _hasLastPosition = false;
        }

        public void ResetPose()
        {
            _wheelSpin = 0f;
            _hasLastPosition = false;
            if (_wheels == null)
            {
                return;
            }

            for (int i = 0; i < _wheels.Length; i++)
            {
                if (_wheels[i] != null)
                {
                    _wheels[i].localRotation = Quaternion.identity;
                }
            }
        }

        public void ApplyState(in VehicleState state)
        {
            Vector3 position = new Vector3(state.Position.X, 0f, state.Position.Y);
            transform.SetPositionAndRotation(position, MathConversions.HeadingToRotation(state.Heading));

            if (_wheels != null && _wheels.Length > 0)
            {
                if (_hasLastPosition)
                {
                    Vector3 delta = position - _lastPosition;
                    _wheelSpin += Vector3.Dot(delta, transform.right) / _wheelRadius * Mathf.Rad2Deg;
                }

                float steer = -state.SteerAngle * Mathf.Rad2Deg;
                for (int i = 0; i < _wheels.Length; i++)
                {
                    Transform wheel = _wheels[i];
                    if (wheel == null)
                    {
                        continue;
                    }

                    float wheelSteer = _frontWheels != null && i < _frontWheels.Length && _frontWheels[i]
                        ? steer
                        : 0f;
                    wheel.localRotation = Quaternion.Euler(0f, wheelSteer, _wheelSpin);
                }
            }

            _lastPosition = position;
            _hasLastPosition = true;
        }
    }
}

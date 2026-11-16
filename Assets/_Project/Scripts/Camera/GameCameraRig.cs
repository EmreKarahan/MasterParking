using MasterParking.Core;
using MasterParking.Utilities;
using UnityEngine;

namespace MasterParking.CameraSystem
{
    /// <summary>
    /// Owns the gameplay camera. Two modes mirror the source game: a chase view
    /// that can be orbited, and a level-overview view for planning a park.
    /// </summary>
    public sealed class GameCameraRig : MonoBehaviour
    {
        private const float FollowDistance = 13.5f;
        private const float FollowHeight = 8.5f;
        private const float FollowLookHeight = 1.4f;

        private Camera _camera;
        private LevelBounds _bounds;
        private Vector3 _followPosition;
        private float _followHeading;
        private bool _hasFollowTarget;

        private CameraMode _mode = CameraMode.Follow;
        private float _orbitYaw;
        private float _orbitPitch;
        private Vector3 _overviewPan;

        private Vector3 _smoothedPosition;
        private Quaternion _smoothedRotation;
        private bool _initialized;

        private float _shakeTime;
        private float _shakeDuration;
        private float _shakeMagnitude;

        public CameraMode Mode => _mode;
        public Camera Camera => _camera;

        public void Initialize(LevelBounds bounds, CameraMode initialMode = CameraMode.Follow)
        {
            _bounds = bounds;
            _mode = initialMode;

            var cameraObject = new GameObject("GameplayCamera");
            cameraObject.transform.SetParent(transform, false);
            _camera = cameraObject.AddComponent<Camera>();
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.05f, 0.07f, 0.1f);
            _camera.fieldOfView = 48f;
            _camera.nearClipPlane = 0.3f;
            _camera.farClipPlane = 400f;

            _smoothedPosition = DesiredPosition(out Quaternion rotation);
            _smoothedRotation = rotation;
            _initialized = true;
            ApplyTransform();
        }

        public void Track(in VehicleState state)
        {
            _followPosition = MathConversions.ToWorld(state.Position);
            _followHeading = state.Heading;
            _hasFollowTarget = true;
        }

        public void ToggleMode()
        {
            SetMode(_mode == CameraMode.Follow ? CameraMode.Overview : CameraMode.Follow);
        }

        public void SetMode(CameraMode mode)
        {
            if (_mode == mode)
            {
                return;
            }

            _mode = mode;
            _overviewPan = Vector3.zero;
        }

        public void AddDrag(Vector2 delta)
        {
            if (_mode == CameraMode.Follow)
            {
                _orbitYaw = Mathf.Clamp(_orbitYaw + delta.x * 0.25f, -80f, 80f);
                _orbitPitch = Mathf.Clamp(_orbitPitch - delta.y * 0.08f, -2.5f, 4.5f);
            }
            else
            {
                float scale = Mathf.Max(_bounds.Width, _bounds.Depth) / 1600f;
                _overviewPan += new Vector3(-delta.x * scale, 0f, -delta.y * scale);
            }
        }

        public void ResetOrbit()
        {
            _orbitYaw = 0f;
            _orbitPitch = 0f;
        }

        public void Shake(float duration, float magnitude)
        {
            _shakeDuration = Mathf.Max(0.01f, duration);
            _shakeTime = _shakeDuration;
            _shakeMagnitude = magnitude;
        }

        public void ApplyTransform()
        {
            if (_camera == null)
            {
                return;
            }

            _camera.transform.SetPositionAndRotation(_smoothedPosition, _smoothedRotation);
        }

        private void LateUpdate()
        {
            if (!_initialized || _camera == null)
            {
                return;
            }

            Vector3 desiredPosition = DesiredPosition(out Quaternion desiredRotation);
            float blend = 1f - Mathf.Exp(-8f * Time.deltaTime);
            _smoothedPosition = Vector3.Lerp(_smoothedPosition, desiredPosition, blend);
            _smoothedRotation = Quaternion.Slerp(_smoothedRotation, desiredRotation, blend);

            Vector3 shake = Vector3.zero;
            if (_shakeTime > 0f)
            {
                _shakeTime -= Time.deltaTime;
                float falloff = Mathf.Clamp01(_shakeTime / _shakeDuration);
                shake = Random.insideUnitSphere * (_shakeMagnitude * falloff);
            }

            _camera.transform.SetPositionAndRotation(_smoothedPosition + shake, _smoothedRotation);
        }

        private Vector3 DesiredPosition(out Quaternion rotation)
        {
            if (_mode == CameraMode.Overview)
            {
                Vector3 center = new Vector3(_bounds.CenterX, 0f, _bounds.CenterZ) + _overviewPan;
                float aspect = _camera != null && _camera.aspect > 0.01f ? _camera.aspect : 16f / 9f;
                float vertical = 2f * Mathf.Tan(_camera != null ? _camera.fieldOfView * Mathf.Deg2Rad * 0.5f : 0.42f);
                float horizontal = vertical * aspect;
                float distance = Mathf.Max(
                    _bounds.Width / Mathf.Max(0.2f, horizontal),
                    _bounds.Depth / Mathf.Max(0.2f, vertical)) * 1.2f;
                distance = Mathf.Clamp(distance, 28f, 150f);

                rotation = Quaternion.Euler(70f, 0f, 0f);
                return center + rotation * (Vector3.back * distance);
            }

            if (!_hasFollowTarget)
            {
                _followPosition = new Vector3(_bounds.CenterX, 0f, _bounds.CenterZ);
            }

            float yaw = _followHeading + _orbitYaw * Mathf.Deg2Rad;
            Vector3 forward = new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw));
            Vector3 position = _followPosition - forward * FollowDistance +
                               Vector3.up * (FollowHeight + _orbitPitch);
            Vector3 lookAt = _followPosition + forward * 2.5f + Vector3.up * FollowLookHeight;
            rotation = Quaternion.LookRotation(lookAt - position, Vector3.up);
            return position;
        }
    }
}

using UnityEngine;

namespace EchoThief
{
    /// <summary>
    /// Simple smooth camera follow for top-down view.
    /// Attach to the Main Camera. No Cinemachine required.
    /// Supports dynamic zoom changes for different room sizes.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform _target;
        
        [Tooltip("Camera stays this far above the target")]
        [SerializeField] private Vector3 _offset = new Vector3(0, 20, 0);
        
        [Tooltip("Lower = more responsive, Higher = more smooth/laggy")]
        [SerializeField] private float _smoothTime = 0.3f;
        
        [Header("Dynamic Zoom")]
        [Tooltip("Smoothing time when transitioning between zoom levels")]
        [SerializeField] private float _zoomTransitionTime = 1.0f;
        
        [Header("Bounds")]
        [Tooltip("Camera won't go outside these XZ bounds")]
        [SerializeField] private bool _useBounds = true;
        [SerializeField] private float _minX = -15f;
        [SerializeField] private float _maxX = 15f;
        [SerializeField] private float _minZ = -15f;
        [SerializeField] private float _maxZ = 15f;

        private Vector3 _velocity = Vector3.zero;
        private Vector3 _targetOffset;
        private Vector3 _offsetVelocity = Vector3.zero;

        private void Awake()
        {
            _targetOffset = _offset;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Smooth transition for dynamic offset changes
            _offset = Vector3.SmoothDamp(_offset, _targetOffset, ref _offsetVelocity, _zoomTransitionTime);

            // Compute desired position
            Vector3 targetPos = _target.position + _offset;

            // Apply bounds if enabled
            if (_useBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, _minX, _maxX);
                targetPos.z = Mathf.Clamp(targetPos.z, _minZ, _maxZ);
            }

            // Smooth damp toward target
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPos, 
                ref _velocity, 
                _smoothTime
            );
        }

        /// <summary>
        /// Dynamically change camera height (zoom level).
        /// Smoothly transitions over _zoomTransitionTime.
        /// Example: SetZoom(35f) for a larger room.
        /// </summary>
        public void SetZoom(float height)
        {
            _targetOffset = new Vector3(_offset.x, height, _offset.z);
        }

        /// <summary>
        /// Dynamically change the full offset (height + XZ position).
        /// Useful for special camera angles or room transitions.
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            _targetOffset = newOffset;
        }

        /// <summary>
        /// Update camera bounds dynamically when entering a different sized room.
        /// Example: SetBounds(-30, 30, -30, 30) for a 60x60 room.
        /// </summary>
        public void SetBounds(float minX, float maxX, float minZ, float maxZ)
        {
            _minX = minX;
            _maxX = maxX;
            _minZ = minZ;
            _maxZ = maxZ;
        }

        /// <summary>
        /// Enable or disable bounds checking.
        /// </summary>
        public void SetBoundsEnabled(bool enabled)
        {
            _useBounds = enabled;
        }

        /// <summary>
        /// Instantly snap to target position without smoothing (useful on scene load).
        /// </summary>
        public void SnapToTarget()
        {
            if (_target == null) return;
            transform.position = _target.position + _offset;
            _velocity = Vector3.zero;
            _offsetVelocity = Vector3.zero;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_useBounds) return;

            // Draw bounds in Scene view
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (_minX + _maxX) / 2f, 
                transform.position.y, 
                (_minZ + _maxZ) / 2f
            );
            Vector3 size = new Vector3(_maxX - _minX, 0.1f, _maxZ - _minZ);
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}

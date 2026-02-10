using UnityEngine;
using EchoThief.Core;

namespace EchoThief.Player
{
    /// <summary>
    /// Generic noise emitter component. Attach to anything that can produce noise
    /// (player, throwable objects, environmental triggers).
    /// 
    /// Can be triggered manually via EmitNoise() or automatically on collision.
    /// </summary>
    public class NoiseEmitter : MonoBehaviour
    {
        [Header("Noise Settings")]
        [Tooltip("Loudness of the noise (0 = silent, 1 = max).")]
        [SerializeField] private float _loudness = 0.5f;

        [Tooltip("Sonar radius this noise produces.")]
        [SerializeField] private float _sonarRadius = 10f;

        [Tooltip("Color of the sonar ring.")]
        [SerializeField] private Color _sonarColor = new Color(1f, 0f, 1f, 1f); // Magenta default

        [Header("Collision Noise")]
        [Tooltip("If true, emit noise on collision (useful for throwable objects).")]
        [SerializeField] private bool _emitOnCollision = false;

        [Tooltip("Minimum impact velocity to trigger noise on collision.")]
        [SerializeField] private float _minImpactVelocity = 2f;

        [Tooltip("Destroy this object after collision noise? (for throwables)")]
        [SerializeField] private bool _destroyAfterCollision = false;

        [SerializeField] private float _destroyDelay = 3f;

        /// <summary>
        /// Emit a noise event at this object's position with the configured settings.
        /// </summary>
        public void EmitNoise()
        {
            NoiseEventBus.EmitNoise(new NoiseEvent(
                origin: transform.position,
                loudness: _loudness,
                sonarRadius: _sonarRadius,
                sonarColor: _sonarColor,
                source: gameObject
            ));
        }

        /// <summary>
        /// Emit a noise event with custom parameters.
        /// </summary>
        public void EmitNoise(float loudness, float radius, Color color)
        {
            NoiseEventBus.EmitNoise(new NoiseEvent(
                origin: transform.position,
                loudness: loudness,
                sonarRadius: radius,
                sonarColor: color,
                source: gameObject
            ));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_emitOnCollision) return;

            if (collision.relativeVelocity.magnitude >= _minImpactVelocity)
            {
                // Use the contact point as the noise origin for more accuracy
                Vector3 contactPoint = collision.contacts.Length > 0
                    ? collision.contacts[0].point
                    : transform.position;

                NoiseEventBus.EmitNoise(new NoiseEvent(
                    origin: contactPoint,
                    loudness: _loudness,
                    sonarRadius: _sonarRadius,
                    sonarColor: _sonarColor,
                    source: gameObject
                ));

                Debug.Log($"[NoiseEmitter] Collision noise at {contactPoint}");

                if (_destroyAfterCollision)
                {
                    Destroy(gameObject, _destroyDelay);
                }
            }
        }
    }
}

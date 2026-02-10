using UnityEngine;
using EchoThief.Core;

namespace EchoThief.Environment
{
    /// <summary>
    /// Periodically emits small sonar pings to provide free ambient visibility.
    /// Attach to environmental objects like dripping pipes, ticking clocks, etc.
    /// </summary>
    public class AmbientNoiseSource : MonoBehaviour
    {
        [Header("Ping Settings")]
        [Tooltip("Time between ambient pings (seconds).")]
        [SerializeField] private float _interval = 3f;

        [Tooltip("Random variance added to interval (±seconds).")]
        [SerializeField] private float _intervalVariance = 0.5f;

        [Tooltip("Sonar radius of the ambient ping.")]
        [SerializeField] private float _sonarRadius = 4f;

        [Tooltip("Loudness (ambient pings should be low — guards shouldn't react to these).")]
        [SerializeField] private float _loudness = 0.05f;

        [Tooltip("Color of the ambient sonar ring.")]
        [SerializeField] private Color _sonarColor = new Color(0f, 0.9f, 0.46f, 1f); // Dim green

        [Header("Audio (Optional)")]
        [Tooltip("Sound effect to play with each ping.")]
        [SerializeField] private AudioClip _pingSound;

        [SerializeField] private AudioSource _audioSource;

        private float _timer;

        private void Start()
        {
            // Randomize start time so not all ambient sources ping simultaneously
            _timer = Random.Range(0f, _interval);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                Ping();
                _timer = _interval + Random.Range(-_intervalVariance, _intervalVariance);
            }
        }

        private void Ping()
        {
            NoiseEventBus.EmitNoise(new NoiseEvent(
                origin: transform.position,
                loudness: _loudness,
                sonarRadius: _sonarRadius,
                sonarColor: _sonarColor,
                source: gameObject
            ));

            // Play ambient sound if configured
            if (_pingSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_pingSound);
            }
        }

        /// <summary>
        /// Draw the ping radius in the editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _sonarColor;
            Gizmos.DrawWireSphere(transform.position, _sonarRadius);
        }
    }
}

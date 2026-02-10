using UnityEngine;
using System.Collections.Generic;
using EchoThief.Core;

namespace EchoThief.Sonar
{
    /// <summary>
    /// Manages all active sonar pulses. Listens to NoiseEventBus, spawns pulses,
    /// updates them each frame, and pushes data to the GPU via global shader properties.
    /// 
    /// Attach this to a single persistent GameObject in the scene.
    /// </summary>
    public class SonarManager : MonoBehaviour
    {
        public static SonarManager Instance { get; private set; }

        [Header("Pulse Defaults")]
        [Tooltip("How fast the sonar ring expands (units/sec).")]
        [SerializeField] private float _defaultExpansionSpeed = 12f;

        [Tooltip("Width of the visible ring band.")]
        [SerializeField] private float _defaultRingThickness = 1.5f;

        [Tooltip("How long a pulse lives before fading out (seconds).")]
        [SerializeField] private float _defaultMaxAge = 2.5f;

        [Header("Limits")]
        [Tooltip("Maximum simultaneous pulses. Shader array size must match.")]
        [SerializeField] private int _maxPulses = 20;

        private readonly List<SonarPulse> _activePulses = new List<SonarPulse>();

        // Shader property IDs (cached for performance)
        private static readonly int PulseCountId = Shader.PropertyToID("_SonarPulseCount");
        private static readonly int PulseOriginsId = Shader.PropertyToID("_SonarPulseOrigins");
        private static readonly int PulseRadiiId = Shader.PropertyToID("_SonarPulseRadii");
        private static readonly int PulseThicknessId = Shader.PropertyToID("_SonarPulseThickness");
        private static readonly int PulseFadesId = Shader.PropertyToID("_SonarPulseFades");
        private static readonly int PulseColorsId = Shader.PropertyToID("_SonarPulseColors");

        // Reusable arrays to avoid GC allocation every frame
        private Vector4[] _origins;
        private float[] _radii;
        private float[] _thickness;
        private float[] _fades;
        private Vector4[] _colors;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Allocate arrays
            _origins = new Vector4[_maxPulses];
            _radii = new float[_maxPulses];
            _thickness = new float[_maxPulses];
            _fades = new float[_maxPulses];
            _colors = new Vector4[_maxPulses];
        }

        private void OnEnable()
        {
            NoiseEventBus.OnNoise += HandleNoiseEvent;
        }

        private void OnDisable()
        {
            NoiseEventBus.OnNoise -= HandleNoiseEvent;
        }

        private void Update()
        {
            // Update all pulses
            for (int i = _activePulses.Count - 1; i >= 0; i--)
            {
                _activePulses[i].Update(Time.deltaTime);

                if (!_activePulses[i].IsAlive)
                {
                    _activePulses.RemoveAt(i);
                }
            }

            // Push data to the shader
            PushToShader();
        }

        /// <summary>
        /// Spawn a new sonar pulse from a noise event.
        /// </summary>
        private void HandleNoiseEvent(NoiseEvent noise)
        {
            SpawnPulse(noise.Origin, noise.SonarRadius, noise.SonarColor);
        }

        /// <summary>
        /// Manually spawn a pulse (e.g., from ambient sources).
        /// </summary>
        public void SpawnPulse(Vector3 origin, float maxRadius, Color color)
        {
            if (_activePulses.Count >= _maxPulses)
            {
                // Remove the oldest pulse to make room
                _activePulses.RemoveAt(0);
            }

            var pulse = new SonarPulse(
                origin,
                maxRadius,
                _defaultExpansionSpeed,
                _defaultRingThickness,
                _defaultMaxAge,
                color
            );

            _activePulses.Add(pulse);
        }

        /// <summary>
        /// Push all active pulse data to global shader properties.
        /// The sonar shader reads these arrays to render the rings.
        /// </summary>
        private void PushToShader()
        {
            int count = Mathf.Min(_activePulses.Count, _maxPulses);

            for (int i = 0; i < count; i++)
            {
                var p = _activePulses[i];
                _origins[i] = new Vector4(p.Origin.x, p.Origin.y, p.Origin.z, 0);
                _radii[i] = p.CurrentRadius;
                _thickness[i] = p.RingThickness;
                _fades[i] = p.FadeFactor;
                _colors[i] = new Vector4(p.Color.r, p.Color.g, p.Color.b, p.Color.a);
            }

            // Zero out unused slots
            for (int i = count; i < _maxPulses; i++)
            {
                _origins[i] = Vector4.zero;
                _radii[i] = 0f;
                _thickness[i] = 0f;
                _fades[i] = 0f;
                _colors[i] = Vector4.zero;
            }

            Shader.SetGlobalInt(PulseCountId, count);
            Shader.SetGlobalVectorArray(PulseOriginsId, _origins);
            Shader.SetGlobalFloatArray(PulseRadiiId, _radii);
            Shader.SetGlobalFloatArray(PulseThicknessId, _thickness);
            Shader.SetGlobalFloatArray(PulseFadesId, _fades);
            Shader.SetGlobalVectorArray(PulseColorsId, _colors);
        }
    }
}

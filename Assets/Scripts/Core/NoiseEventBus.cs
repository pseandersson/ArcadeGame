using UnityEngine;
using System;

namespace EchoThief.Core
{
    /// <summary>
    /// Data payload for a noise event. Every sound in the game flows through this.
    /// </summary>
    public struct NoiseEvent
    {
        /// <summary>World-space origin of the noise.</summary>
        public Vector3 Origin;

        /// <summary>How loud the noise is (0 = silent, 1 = max). Affects sonar radius and guard hearing range.</summary>
        public float Loudness;

        /// <summary>Maximum sonar radius this noise produces.</summary>
        public float SonarRadius;

        /// <summary>Color tint for the sonar ring.</summary>
        public Color SonarColor;

        /// <summary>The GameObject that produced the noise (for guards to identify source type).</summary>
        public GameObject Source;

        public NoiseEvent(Vector3 origin, float loudness, float sonarRadius, Color sonarColor, GameObject source = null)
        {
            Origin = origin;
            Loudness = loudness;
            SonarRadius = sonarRadius;
            SonarColor = sonarColor;
            Source = source;
        }
    }

    /// <summary>
    /// Static event bus for noise events. Decouples noise producers (player, environment)
    /// from consumers (SonarManager, GuardHearing).
    /// 
    /// Usage:
    ///   Producer:  NoiseEventBus.EmitNoise(new NoiseEvent(...));
    ///   Consumer:  NoiseEventBus.OnNoise += HandleNoise;
    /// </summary>
    public static class NoiseEventBus
    {
        /// <summary>
        /// Subscribe to receive all noise events in the game.
        /// </summary>
        public static event Action<NoiseEvent> OnNoise;

        /// <summary>
        /// Emit a noise event. All subscribers (SonarManager, guards, etc.) will be notified.
        /// </summary>
        public static void EmitNoise(NoiseEvent noiseEvent)
        {
            OnNoise?.Invoke(noiseEvent);
        }

        /// <summary>
        /// Clear all subscribers. Call this on scene unload to prevent stale references.
        /// </summary>
        public static void ClearAll()
        {
            OnNoise = null;
        }
    }
}

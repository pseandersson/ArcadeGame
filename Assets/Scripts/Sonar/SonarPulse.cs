using UnityEngine;

namespace EchoThief.Sonar
{
    /// <summary>
    /// Represents a single expanding sonar pulse ring.
    /// Created by SonarManager when a noise event occurs.
    /// This is a plain data class (not a MonoBehaviour).
    /// </summary>
    [System.Serializable]
    public class SonarPulse
    {
        /// <summary>World-space origin of the pulse.</summary>
        public Vector3 Origin;

        /// <summary>Current expanding radius.</summary>
        public float CurrentRadius;

        /// <summary>Maximum radius before the pulse stops expanding.</summary>
        public float MaxRadius;

        /// <summary>How fast the radius expands (units/second).</summary>
        public float ExpansionSpeed;

        /// <summary>Width of the visible ring band.</summary>
        public float RingThickness;

        /// <summary>Time since the pulse was created.</summary>
        public float Age;

        /// <summary>How long the pulse lives before fully fading out.</summary>
        public float MaxAge;

        /// <summary>Neon color tint of this pulse.</summary>
        public Color Color;

        /// <summary>Is this pulse still active?</summary>
        public bool IsAlive => Age < MaxAge;

        /// <summary>Fade factor (1 = full, 0 = gone).</summary>
        public float FadeFactor => Mathf.Clamp01(1f - (Age / MaxAge));

        public SonarPulse(Vector3 origin, float maxRadius, float expansionSpeed, float ringThickness, float maxAge, Color color)
        {
            Origin = origin;
            CurrentRadius = 0f;
            MaxRadius = maxRadius;
            ExpansionSpeed = expansionSpeed;
            RingThickness = ringThickness;
            Age = 0f;
            MaxAge = maxAge;
            Color = color;
        }

        /// <summary>
        /// Advance the pulse by deltaTime. Call once per frame.
        /// </summary>
        public void Update(float deltaTime)
        {
            Age += deltaTime;
            CurrentRadius = Mathf.Min(CurrentRadius + ExpansionSpeed * deltaTime, MaxRadius);
        }
    }
}

using UnityEngine;

namespace EchoThief.Sonar
{
    [System.Serializable]
    public struct SoundWave
    {
        public Vector2 Origin;
        public float Radius;
        public float InitialRadius;
        public float MaxRadius;
        public float InitialLoudness;
        public int Depth;
        public float ArcStart;
        public float ArcEnd;
        public Color Color;

        public SoundWave(Vector2 origin, float currentRadius, float initialRadius, float maxRadius, float initialLoudness, int depth, float arcStart, float arcEnd, Color color)
        {
            Origin = origin;
            Radius = currentRadius;
            InitialRadius = initialRadius;
            MaxRadius = maxRadius;
            InitialLoudness = initialLoudness;
            Depth = depth;
            ArcStart = arcStart;
            ArcEnd = arcEnd;
            Color = color;
        }

        public float CurrentLoudness
        {
            get
            {
                float expansion = Mathf.Max(Radius - InitialRadius, 0f);
                return InitialLoudness / Mathf.Max(expansion * expansion, 1f);
            }
        }

        public float Fade => Mathf.Clamp01(CurrentLoudness);

        public bool IsAlive(float loudnessThreshold)
        {
            return Radius <= MaxRadius && CurrentLoudness >= loudnessThreshold;
        }

        public void Advance(float deltaTime, float expansionSpeed)
        {
            Radius = Mathf.Min(Radius + expansionSpeed * deltaTime, MaxRadius);
        }
    }
}

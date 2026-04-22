using System;

namespace SDAS.Runtime.Solver
{
    [Serializable]
    public class FallbackPolicy
    {
        public int maxAttempts = 3;
        public float maxOverlapRatioStep = 0.05f;
        public float minSafetyDistanceStep = 0.05f;
        public float rotationWeightStep = 0.5f;

        public SolverConfig CreateAttemptConfig(SolverConfig baseConfig, int attempt)
        {
            var cfg = new SolverConfig
            {
                weights = new SolverWeights
                {
                    link = baseConfig.weights.link,
                    overlap = baseConfig.weights.overlap,
                    safety = baseConfig.weights.safety,
                    gravity = baseConfig.weights.gravity,
                    rotation = Math.Max(0f, baseConfig.weights.rotation - rotationWeightStep * attempt)
                },
                maxOverlapRatio = baseConfig.maxOverlapRatio + maxOverlapRatioStep * attempt,
                minSafetyDistance = Math.Max(0.05f, baseConfig.minSafetyDistance - minSafetyDistanceStep * attempt),
                translationStep = baseConfig.translationStep,
                rotationStepDeg = baseConfig.rotationStepDeg,
                maxTranslationSamplesPerAxis = baseConfig.maxTranslationSamplesPerAxis,
                maxRotationSamples = baseConfig.maxRotationSamples
            };

            return cfg;
        }
    }
}

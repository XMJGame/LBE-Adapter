using NUnit.Framework;
using SDAS.Runtime.Solver;

namespace SDAS.Tests
{
    public class SolverFallbackTests
    {
        [Test]
        public void CreateAttemptConfig_AdjustsThresholdsByAttempt()
        {
            var baseConfig = new SolverConfig
            {
                maxOverlapRatio = 0.1f,
                minSafetyDistance = 0.4f,
                weights = new SolverWeights { rotation = 4f }
            };

            var policy = new FallbackPolicy
            {
                maxOverlapRatioStep = 0.05f,
                minSafetyDistanceStep = 0.1f,
                rotationWeightStep = 0.5f
            };

            var adjusted = policy.CreateAttemptConfig(baseConfig, 2);
            Assert.AreEqual(0.2f, adjusted.maxOverlapRatio, 0.0001f);
            Assert.AreEqual(0.2f, adjusted.minSafetyDistance, 0.0001f);
            Assert.AreEqual(3f, adjusted.weights.rotation, 0.0001f);
        }
    }
}

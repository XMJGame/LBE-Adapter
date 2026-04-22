using System.IO;
using NUnit.Framework;
using SDAS.Runtime.Serialization;
using SDAS.Runtime.Solver;
using UnityEngine;

namespace SDAS.Tests
{
    public class SampleScenarioSolverTests
    {
        [Test]
        public void SevenChapter_200sqm_Sample_CanSolveAndExportMappings()
        {
            var samplePath = Path.Combine(Application.dataPath, "SDAS/Samples/SevenChapters_200sqm.json");
            Assert.IsTrue(File.Exists(samplePath), $"Sample json missing: {samplePath}");

            var data = SDASJsonSerializer.LoadFromFile(samplePath);
            Assert.AreEqual(7, data.chapters.Count, "Sample should contain 7 chapters.");
            Assert.AreEqual(4, data.siteData.boundary.Count, "Sample boundary should be rectangle with 4 points.");

            var config = new SolverConfig
            {
                translationStep = 0.25f,
                maxTranslationSamplesPerAxis = 32,
                rotationStepDeg = 2f,
                maxRotationSamples = 24,
                enableGlobalRefinement = true,
                annealingIterations = 150
            };

            var orchestrator = new SolverOrchestrator();
            var fallback = new FallbackPolicy { maxAttempts = 3 };
            var result = orchestrator.SolveWithFallback(data.chapters, data.siteData, config, fallback, out var attempts);

            Assert.IsTrue(result.success, $"Solver failed: {result.message}");
            Assert.AreEqual(7, result.mappingResults.Count, "All 7 chapters should have mapping results.");
            Assert.GreaterOrEqual(attempts, 1);

            var outPath = Path.Combine(Application.dataPath, "SDAS/Samples/SevenChapters_200sqm.Result.json");
            data.mappingResult = result.mappingResults;
            SDASJsonSerializer.SaveToFile(outPath, data, true);
            Assert.IsTrue(File.Exists(outPath), "Result json should be exported.");
        }
    }
}

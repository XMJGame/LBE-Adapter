using System.Collections.Generic;
using SDAS.Runtime.Core;

namespace SDAS.Runtime.Solver
{
    /// <summary>
    /// 封装求解 + fallback 策略。
    /// </summary>
    public class SolverOrchestrator
    {
        private readonly GreedyChapterSolver greedySolver = new();
        private readonly SimulatedAnnealingRefiner refiner = new();

        public SolverResult SolveWithFallback(
            IReadOnlyList<ChapterNode> chapters,
            SiteData siteData,
            SolverConfig baseConfig,
            FallbackPolicy fallback,
            out int usedAttempts)
        {
            usedAttempts = 0;
            baseConfig ??= new SolverConfig();
            fallback ??= new FallbackPolicy();

            SolverResult latest = null;
            for (var attempt = 0; attempt < fallback.maxAttempts; attempt++)
            {
                var attemptConfig = fallback.CreateAttemptConfig(baseConfig, attempt);
                latest = greedySolver.Solve(chapters, siteData, attemptConfig);
                usedAttempts = attempt + 1;

                if (latest.success)
                {
                    if (attemptConfig.enableGlobalRefinement)
                    {
                        var refined = refiner.Refine(latest.placements, chapters, siteData, attemptConfig);
                        latest.placements = refined;
                        latest.mappingResults.Clear();
                        for (var i = 0; i < refined.Count; i++)
                        {
                            latest.mappingResults.Add(refined[i].ToMappingResult());
                        }
                    }

                    latest.message = $"Solved in attempt {usedAttempts}.";
                    return latest;
                }
            }

            if (latest == null)
            {
                latest = new SolverResult { success = false, message = "Solver failed without producing a result." };
            }
            else
            {
                latest.message = $"Solver failed after {usedAttempts} attempts with fallback.";
            }

            return latest;
        }
    }
}

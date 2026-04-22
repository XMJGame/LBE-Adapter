using System;
using System.Collections.Generic;
using SDAS.Runtime.Core;
using UnityEngine;

namespace SDAS.Runtime.Solver
{
    /// <summary>
    /// Step 3：在贪婪初始解基础上做模拟退火微调。
    /// </summary>
    public class SimulatedAnnealingRefiner
    {
        private readonly System.Random random;

        public SimulatedAnnealingRefiner(int? seed = null)
        {
            random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        public List<ChapterPlacement> Refine(
            IReadOnlyList<ChapterPlacement> initial,
            IReadOnlyList<ChapterNode> chapters,
            SiteData siteData,
            SolverConfig config)
        {
            if (initial == null || initial.Count == 0)
            {
                return new List<ChapterPlacement>();
            }

            var current = ClonePlacements(initial);
            var best = ClonePlacements(initial);

            var currentScore = EvaluateTotal(current, chapters, siteData, config);
            var bestScore = currentScore;
            var temperature = Mathf.Max(0.0001f, config.annealingStartTemperature);

            for (var iter = 0; iter < Mathf.Max(1, config.annealingIterations); iter++)
            {
                var candidate = ProposeNeighbor(current, config);
                var candidateScore = EvaluateTotal(candidate, chapters, siteData, config);

                var improved = candidateScore < currentScore;
                if (improved || AcceptWorseMove(currentScore, candidateScore, temperature))
                {
                    current = candidate;
                    currentScore = candidateScore;
                }

                if (currentScore < bestScore)
                {
                    best = ClonePlacements(current);
                    bestScore = currentScore;
                }

                temperature *= Mathf.Clamp(config.annealingCoolingRate, 0.8f, 0.9999f);
            }

            return best;
        }

        private static float EvaluateTotal(
            IReadOnlyList<ChapterPlacement> placements,
            IReadOnlyList<ChapterNode> chapters,
            SiteData siteData,
            SolverConfig config)
        {
            float total = 0f;
            var accepted = new List<ChapterPlacement>();

            for (var i = 0; i < placements.Count; i++)
            {
                var chapter = FindChapterById(chapters, placements[i].chapterId);
                if (chapter == null)
                {
                    total += 10_000f;
                    continue;
                }

                var costs = SolverCostEvaluator.Evaluate(
                    chapter,
                    placements[i].worldPos,
                    placements[i].worldRotEuler.y,
                    accepted,
                    chapters,
                    siteData,
                    config);

                total += costs.Total(config.weights);
                accepted.Add(new ChapterPlacement
                {
                    chapterId = placements[i].chapterId,
                    worldPos = placements[i].worldPos,
                    worldRotEuler = placements[i].worldRotEuler,
                    costs = costs
                });
            }

            return total;
        }

        private List<ChapterPlacement> ProposeNeighbor(IReadOnlyList<ChapterPlacement> current, SolverConfig config)
        {
            var next = ClonePlacements(current);
            var idx = random.Next(0, next.Count);
            var selected = next[idx];

            var dx = NextRange(-1f, 1f) * config.translationStep;
            var dz = NextRange(-1f, 1f) * config.translationStep;
            var dyaw = NextRange(-1f, 1f) * config.rotationStepDeg;

            selected.worldPos += new Vector3(dx, 0f, dz);
            selected.worldRotEuler = new Vector3(0f, selected.worldRotEuler.y + dyaw, 0f);
            next[idx] = selected;
            return next;
        }

        private bool AcceptWorseMove(float current, float candidate, float temperature)
        {
            var delta = candidate - current;
            var probability = Mathf.Exp(-delta / Mathf.Max(0.0001f, temperature));
            return random.NextDouble() < probability;
        }

        private static List<ChapterPlacement> ClonePlacements(IReadOnlyList<ChapterPlacement> src)
        {
            var output = new List<ChapterPlacement>(src.Count);
            for (var i = 0; i < src.Count; i++)
            {
                output.Add(new ChapterPlacement
                {
                    chapterId = src[i].chapterId,
                    worldPos = src[i].worldPos,
                    worldRotEuler = src[i].worldRotEuler,
                    costs = src[i].costs == null
                        ? new CostBreakdown()
                        : new CostBreakdown
                        {
                            link = src[i].costs.link,
                            overlap = src[i].costs.overlap,
                            safety = src[i].costs.safety,
                            gravity = src[i].costs.gravity,
                            rotation = src[i].costs.rotation
                        }
                });
            }

            return output;
        }

        private static ChapterNode FindChapterById(IReadOnlyList<ChapterNode> chapters, int chapterId)
        {
            for (var i = 0; i < chapters.Count; i++)
            {
                if (chapters[i].chapterId == chapterId)
                {
                    return chapters[i];
                }
            }

            return null;
        }

        private float NextRange(float min, float max)
        {
            return (float)(min + (max - min) * random.NextDouble());
        }
    }
}

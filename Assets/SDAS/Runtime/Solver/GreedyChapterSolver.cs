using System.Collections.Generic;
using SDAS.Runtime.Core;
using UnityEngine;

namespace SDAS.Runtime.Solver
{
    /// <summary>
    /// Step 2: 基础贪婪求解器（按章节顺序搜索最小代价位置）。
    /// 后续可在此基础上叠加全局优化。
    /// </summary>
    public class GreedyChapterSolver
    {
        public SolverResult Solve(IReadOnlyList<ChapterNode> chapters, SiteData siteData, SolverConfig config)
        {
            var result = new SolverResult();
            if (chapters == null || chapters.Count == 0)
            {
                result.success = false;
                result.message = "No chapters provided.";
                return result;
            }

            config ??= new SolverConfig();
            var placements = new List<ChapterPlacement>();

            for (var i = 0; i < chapters.Count; i++)
            {
                var chapter = chapters[i];
                var best = FindBestPlacement(chapter, placements, chapters, siteData, config);
                if (best == null)
                {
                    result.success = false;
                    result.message = $"Failed to place chapter {chapter.chapterId}.";
                    return result;
                }

                placements.Add(best);
            }

            result.success = true;
            result.message = "Greedy solve completed.";
            result.placements = placements;
            for (var i = 0; i < placements.Count; i++)
            {
                result.mappingResults.Add(placements[i].ToMappingResult());
            }

            return result;
        }

        private ChapterPlacement FindBestPlacement(
            ChapterNode chapter,
            IReadOnlyList<ChapterPlacement> accepted,
            IReadOnlyList<ChapterNode> allChapters,
            SiteData siteData,
            SolverConfig config)
        {
            ChapterPlacement best = null;
            var bestScore = float.PositiveInfinity;

            var yawExtent = config.rotationStepDeg * config.maxRotationSamples * 0.5f;
            for (var sx = -config.maxTranslationSamplesPerAxis; sx <= config.maxTranslationSamplesPerAxis; sx++)
            {
                for (var sz = -config.maxTranslationSamplesPerAxis; sz <= config.maxTranslationSamplesPerAxis; sz++)
                {
                    var candidatePos = new Vector3(sx * config.translationStep, 0f, sz * config.translationStep);

                    for (var r = 0; r < config.maxRotationSamples; r++)
                    {
                        var yaw = -yawExtent + (2f * yawExtent * r / Mathf.Max(1, config.maxRotationSamples - 1));
                        var costs = SolverCostEvaluator.Evaluate(chapter, candidatePos, yaw, accepted, allChapters, siteData, config);
                        var total = costs.Total(config.weights);
                        if (total < bestScore)
                        {
                            bestScore = total;
                            best = new ChapterPlacement
                            {
                                chapterId = chapter.chapterId,
                                worldPos = candidatePos,
                                worldRotEuler = new Vector3(0f, yaw, 0f),
                                costs = costs
                            };
                        }
                    }
                }
            }

            return best;
        }
    }
}

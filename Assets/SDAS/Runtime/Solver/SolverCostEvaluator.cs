using System.Collections.Generic;
using SDAS.Runtime.Core;
using UnityEngine;

namespace SDAS.Runtime.Solver
{
    public static class SolverCostEvaluator
    {
        public static CostBreakdown Evaluate(
            ChapterNode chapter,
            Vector3 candidatePos,
            float candidateYaw,
            IReadOnlyList<ChapterPlacement> acceptedPlacements,
            IReadOnlyList<ChapterNode> allChapters,
            SiteData siteData,
            SolverConfig config)
        {
            var breakdown = new CostBreakdown();
            var candidatePivot = candidatePos + RotateXZ(chapter.EstimatedPivot(), candidateYaw);

            breakdown.link = ComputeLinkCost(chapter, candidatePivot, acceptedPlacements, allChapters);
            breakdown.overlap = ComputeOverlapCost(chapter, candidatePos, candidateYaw, acceptedPlacements, allChapters, config.maxOverlapRatio);
            breakdown.safety = ComputeSafetyCost(chapter, candidatePos, candidateYaw, siteData, config.minSafetyDistance);
            breakdown.gravity = ComputeGravityCost(candidatePivot, siteData.boundary);
            breakdown.rotation = Mathf.Abs(candidateYaw);

            return breakdown;
        }

        private static float ComputeLinkCost(
            ChapterNode chapter,
            Vector3 candidatePivot,
            IReadOnlyList<ChapterPlacement> acceptedPlacements,
            IReadOnlyList<ChapterNode> allChapters)
        {
            var index = IndexOfChapter(allChapters, chapter.chapterId);
            if (index <= 0 || acceptedPlacements.Count == 0)
            {
                return 0f;
            }

            var prev = acceptedPlacements[acceptedPlacements.Count - 1];
            var dist = Vector3.Distance(candidatePivot, prev.worldPos);
            return dist;
        }

        private static float ComputeOverlapCost(
            ChapterNode chapter,
            Vector3 pos,
            float yaw,
            IReadOnlyList<ChapterPlacement> acceptedPlacements,
            IReadOnlyList<ChapterNode> allChapters,
            float maxOverlapRatio)
        {
            var candidatePolys = BuildChapterPolygons(chapter, pos, yaw);
            if (candidatePolys.Count == 0)
            {
                return 1_000f;
            }

            var overlapCount = 0;
            var totalChecks = 0;

            for (var i = 0; i < acceptedPlacements.Count; i++)
            {
                var acceptedChapter = FindChapterById(allChapters, acceptedPlacements[i].chapterId);
                if (acceptedChapter == null)
                {
                    continue;
                }

                var placedPolys = BuildChapterPolygons(acceptedChapter, acceptedPlacements[i].worldPos, acceptedPlacements[i].worldRotEuler.y);
                for (var a = 0; a < candidatePolys.Count; a++)
                {
                    for (var b = 0; b < placedPolys.Count; b++)
                    {
                        totalChecks++;
                        if (Geometry2D.OverlapSAT(candidatePolys[a], placedPolys[b]))
                        {
                            overlapCount++;
                        }
                    }
                }
            }

            if (totalChecks == 0)
            {
                return 0f;
            }

            var ratio = overlapCount / (float)totalChecks;
            return ratio <= maxOverlapRatio ? 0f : (ratio - maxOverlapRatio) * 100f;
        }

        private static float ComputeSafetyCost(ChapterNode chapter, Vector3 pos, float yaw, SiteData siteData, float minSafetyDistance)
        {
            var polygons = BuildChapterPolygons(chapter, pos, yaw);
            if (polygons.Count == 0)
            {
                return 1_000f;
            }

            float minDistanceToBoundary = float.MaxValue;
            if (siteData.boundary != null && siteData.boundary.Count >= 3)
            {
                var boundary2D = To2D(siteData.boundary);
                for (var i = 0; i < polygons.Count; i++)
                {
                    var poly = polygons[i];
                    for (var p = 0; p < poly.Count; p++)
                    {
                        if (!Geometry2D.IsPointInPolygon(poly[p], boundary2D))
                        {
                            return 10_000f;
                        }

                        minDistanceToBoundary = Mathf.Min(minDistanceToBoundary, Geometry2D.MinDistanceToPolygon(poly[p], boundary2D));
                    }
                }
            }

            var minDistanceToObstacle = float.MaxValue;
            for (var i = 0; i < siteData.obstacles.Count; i++)
            {
                var obstacle2D = To2D(siteData.obstacles[i].polygon);
                if (obstacle2D.Count < 3)
                {
                    continue;
                }

                for (var cp = 0; cp < polygons.Count; cp++)
                {
                    for (var p = 0; p < polygons[cp].Count; p++)
                    {
                        if (Geometry2D.IsPointInPolygon(polygons[cp][p], obstacle2D))
                        {
                            return 10_000f;
                        }

                        minDistanceToObstacle = Mathf.Min(minDistanceToObstacle, Geometry2D.MinDistanceToPolygon(polygons[cp][p], obstacle2D));
                    }
                }
            }

            var minDistance = Mathf.Min(minDistanceToBoundary, minDistanceToObstacle);
            if (float.IsInfinity(minDistance) || minDistance == float.MaxValue)
            {
                return 0f;
            }

            if (minDistance >= minSafetyDistance)
            {
                return 0f;
            }

            var normalizedGap = Mathf.Clamp01((minSafetyDistance - minDistance) / Mathf.Max(0.0001f, minSafetyDistance));
            return normalizedGap * normalizedGap * 500f;
        }

        private static float ComputeGravityCost(Vector3 candidatePivot, List<Vector3> boundary)
        {
            if (boundary == null || boundary.Count == 0)
            {
                return 0f;
            }

            var center = Vector3.zero;
            for (var i = 0; i < boundary.Count; i++)
            {
                center += boundary[i];
            }

            center /= boundary.Count;
            return Vector3.Distance(candidatePivot, center);
        }

        private static List<List<Vector2>> BuildChapterPolygons(ChapterNode chapter, Vector3 worldPos, float yawDeg)
        {
            var result = new List<List<Vector2>>();
            if (chapter.walkingAreas == null)
            {
                return result;
            }

            for (var i = 0; i < chapter.walkingAreas.Count; i++)
            {
                var local = chapter.walkingAreas[i].ToPolygonXZ();
                var transformed = new List<Vector2>(local.Count);
                for (var p = 0; p < local.Count; p++)
                {
                    var local3 = new Vector3(local[p].x, 0f, local[p].y);
                    var rotated = RotateXZ(local3, yawDeg);
                    var world = rotated + worldPos;
                    transformed.Add(new Vector2(world.x, world.z));
                }

                if (transformed.Count >= 3)
                {
                    result.Add(transformed);
                }
            }

            return result;
        }

        private static Vector3 RotateXZ(Vector3 point, float yawDeg)
        {
            var q = Quaternion.Euler(0f, yawDeg, 0f);
            return q * point;
        }

        private static int IndexOfChapter(IReadOnlyList<ChapterNode> chapters, int chapterId)
        {
            for (var i = 0; i < chapters.Count; i++)
            {
                if (chapters[i].chapterId == chapterId)
                {
                    return i;
                }
            }

            return -1;
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

        private static List<Vector2> To2D(List<Vector3> points)
        {
            var result = new List<Vector2>();
            if (points == null)
            {
                return result;
            }

            for (var i = 0; i < points.Count; i++)
            {
                result.Add(new Vector2(points[i].x, points[i].z));
            }

            return result;
        }
    }
}

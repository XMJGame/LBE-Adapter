using System.Collections.Generic;
using UnityEngine;

namespace SDAS.Runtime.Core
{
    /// <summary>
    /// XZ 平面几何工具（第一步实现：基础 SAT 投影与点到线段距离）。
    /// </summary>
    public static class Geometry2D
    {
        public static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var sqrLen = ab.sqrMagnitude;
            if (sqrLen <= Mathf.Epsilon)
            {
                return Vector2.Distance(p, a);
            }

            var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / sqrLen);
            var projection = a + t * ab;
            return Vector2.Distance(p, projection);
        }

        public static bool OverlapSAT(IReadOnlyList<Vector2> polyA, IReadOnlyList<Vector2> polyB)
        {
            return !HasSeparatingAxis(polyA, polyB) && !HasSeparatingAxis(polyB, polyA);
        }

        private static bool HasSeparatingAxis(IReadOnlyList<Vector2> source, IReadOnlyList<Vector2> target)
        {
            if (source == null || target == null || source.Count < 3 || target.Count < 3)
            {
                return true;
            }

            for (var i = 0; i < source.Count; i++)
            {
                var p1 = source[i];
                var p2 = source[(i + 1) % source.Count];
                var edge = p2 - p1;
                var axis = new Vector2(-edge.y, edge.x).normalized;

                ProjectPolygon(source, axis, out var srcMin, out var srcMax);
                ProjectPolygon(target, axis, out var tarMin, out var tarMax);

                if (srcMax < tarMin || tarMax < srcMin)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ProjectPolygon(IReadOnlyList<Vector2> poly, Vector2 axis, out float min, out float max)
        {
            var value = Vector2.Dot(poly[0], axis);
            min = value;
            max = value;

            for (var i = 1; i < poly.Count; i++)
            {
                value = Vector2.Dot(poly[i], axis);
                if (value < min)
                {
                    min = value;
                }

                if (value > max)
                {
                    max = value;
                }
            }
        }
    }
}

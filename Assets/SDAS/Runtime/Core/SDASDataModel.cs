using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDAS.Runtime.Core
{
    public enum AreaShapeType
    {
        Circle = 0,
        Polygon = 1
    }

    public enum ObstacleType
    {
        Wall = 0,
        Pillar = 1,
        Temp = 2
    }

    public enum GatewayType
    {
        Entrance = 0,
        Exit = 1,
        Bidirectional = 2
    }

    [Serializable]
    public class WalkArea
    {
        public AreaShapeType shape = AreaShapeType.Circle;
        public Vector3 center = Vector3.zero;
        public float radius = 1f;
        public List<Vector3> vertices = new();
        public Vector3 forward = Vector3.forward;
        public Vector3 pivot = Vector3.zero;

        /// <summary>
        /// 将区域转换为 XZ 平面的二维点集（用于 SAT / 距离运算）。
        /// Circle 将返回规则多边形近似。
        /// </summary>
        public List<Vector2> ToPolygonXZ(int circleSegments = 24)
        {
            var points = new List<Vector2>();

            if (shape == AreaShapeType.Circle)
            {
                var safeSegments = Mathf.Max(3, circleSegments);
                for (var i = 0; i < safeSegments; i++)
                {
                    var rad = i * Mathf.PI * 2f / safeSegments;
                    var x = center.x + Mathf.Cos(rad) * radius;
                    var z = center.z + Mathf.Sin(rad) * radius;
                    points.Add(new Vector2(x, z));
                }

                return points;
            }

            for (var i = 0; i < vertices.Count; i++)
            {
                points.Add(new Vector2(vertices[i].x, vertices[i].z));
            }

            return points;
        }
    }

    [Serializable]
    public class ChapterNode
    {
        public int chapterId;
        public string chapterName = string.Empty;
        public List<WalkArea> walkingAreas = new();

        public bool isLinkedToNext;
        public bool lockRelativeTransform = true;
        public float posThreshold = 0.3f;
        public float rotThreshold = 8f;

        public Vector3 EstimatedPivot()
        {
            if (walkingAreas == null || walkingAreas.Count == 0)
            {
                return Vector3.zero;
            }

            var acc = Vector3.zero;
            for (var i = 0; i < walkingAreas.Count; i++)
            {
                acc += walkingAreas[i].pivot;
            }

            return acc / walkingAreas.Count;
        }
    }

    [Serializable]
    public class SiteObstacle
    {
        public string obstacleId = string.Empty;
        public ObstacleType obstacleType = ObstacleType.Wall;
        public List<Vector3> polygon = new();
    }

    [Serializable]
    public class SiteGateway
    {
        public string gatewayId = string.Empty;
        public GatewayType gatewayType = GatewayType.Bidirectional;
        public Vector3 position = Vector3.zero;
        public Vector3 forward = Vector3.forward;
        public float attractionWeight = 1f;
    }

    [Serializable]
    public class SiteData
    {
        public List<Vector3> boundary = new();
        public List<SiteObstacle> obstacles = new();
        public List<SiteGateway> gateways = new();
    }

    [Serializable]
    public class ChapterMappingResult
    {
        public int chapterId;
        public Vector3 worldPos = Vector3.zero;
        public Vector3 worldRotEuler = Vector3.zero;
        public Vector3 inverseOffset = Vector3.zero;
        public float inverseYaw;
    }

    [Serializable]
    public class SDASProjectData
    {
        public List<ChapterNode> chapters = new();
        public SiteData siteData = new();
        public List<ChapterMappingResult> mappingResult = new();
    }
}

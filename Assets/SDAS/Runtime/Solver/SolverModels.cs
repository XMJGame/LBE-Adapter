using System;
using System.Collections.Generic;
using SDAS.Runtime.Core;
using UnityEngine;

namespace SDAS.Runtime.Solver
{
    [Serializable]
    public class SolverWeights
    {
        public float link = 3f;
        public float overlap = 5f;
        public float safety = 8f;
        public float gravity = 2f;
        public float rotation = 4f;
    }

    [Serializable]
    public class SolverConfig
    {
        public SolverWeights weights = new();
        public float maxOverlapRatio = 0.15f;
        public float minSafetyDistance = 0.4f;
        public float translationStep = 0.1f;
        public float rotationStepDeg = 1f;
        public int maxTranslationSamplesPerAxis = 20;
        public int maxRotationSamples = 16;
    }

    [Serializable]
    public class CostBreakdown
    {
        public float link;
        public float overlap;
        public float safety;
        public float gravity;
        public float rotation;

        public float Total(SolverWeights w)
        {
            return link * w.link
                 + overlap * w.overlap
                 + safety * w.safety
                 + gravity * w.gravity
                 + rotation * w.rotation;
        }
    }

    [Serializable]
    public class ChapterPlacement
    {
        public int chapterId;
        public Vector3 worldPos;
        public Vector3 worldRotEuler;
        public CostBreakdown costs = new();

        public ChapterMappingResult ToMappingResult()
        {
            var raw = new ChapterMappingResult
            {
                chapterId = chapterId,
                worldPos = worldPos,
                worldRotEuler = worldRotEuler
            };

            return Mapping.MappingMath.BuildInverse(raw);
        }
    }

    [Serializable]
    public class SolverResult
    {
        public bool success;
        public string message = string.Empty;
        public List<ChapterPlacement> placements = new();
        public List<ChapterMappingResult> mappingResults = new();
    }
}

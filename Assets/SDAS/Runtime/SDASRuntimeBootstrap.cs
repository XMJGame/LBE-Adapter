using System.Collections.Generic;
using SDAS.Runtime.Authoring;
using SDAS.Runtime.Core;
using SDAS.Runtime.Mapping;
using SDAS.Runtime.Serialization;
using SDAS.Runtime.Solver;
using UnityEngine;

namespace SDAS.Runtime
{
    /// <summary>
    /// 运行时引导：从 Authoring 或 JSON 执行求解并绑定 SpaceCoordinator。
    /// </summary>
    public class SDASRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private bool useJsonAsInput;
        [SerializeField] private string jsonPath = "sdas_mapping.json";

        [Header("Scene References")]
        [SerializeField] private List<ChapterAuthoring> chapterAuthorings = new();
        [SerializeField] private SiteAuthoring siteAuthoring;
        [SerializeField] private SpaceCoordinator spaceCoordinator;

        [Header("Solver")]
        [SerializeField] private SolverConfig solverConfig = new();
        [SerializeField] private FallbackPolicy fallbackPolicy = new();

        private readonly SolverOrchestrator orchestrator = new();
        private SolverResult latestResult;

        [ContextMenu("Solve And Bind")]
        public void SolveAndBind()
        {
            var data = useJsonAsInput ? LoadFromJson() : BuildDataFromScene();
            var result = orchestrator.SolveWithFallback(data.chapters, data.siteData, solverConfig, fallbackPolicy, out var attempts);
            latestResult = result;

            if (!result.success)
            {
                Debug.LogError($"[SDAS] Solver failed after {attempts} attempts: {result.message}");
                return;
            }

            spaceCoordinator.BindData(data.chapters, result.mappingResults);
            if (!string.IsNullOrWhiteSpace(jsonPath))
            {
                data.mappingResult = result.mappingResults;
                SDASJsonSerializer.SaveToFile(jsonPath, data, true);
            }

            Debug.Log($"[SDAS] Solve success, chapters={result.mappingResults.Count}, attempts={attempts}");
        }

        [ContextMenu("Apply First Chapter")]
        public void ApplyFirstChapter()
        {
            if (latestResult == null || latestResult.mappingResults.Count == 0)
            {
                Debug.LogWarning("[SDAS] No mapping result available.");
                return;
            }

            spaceCoordinator.ApplyChapter(latestResult.mappingResults[0].chapterId);
        }

        private SDASProjectData BuildDataFromScene()
        {
            var data = new SDASProjectData();
            for (var i = 0; i < chapterAuthorings.Count; i++)
            {
                if (chapterAuthorings[i] != null)
                {
                    data.chapters.Add(chapterAuthorings[i].chapter);
                }
            }

            data.siteData = siteAuthoring != null ? siteAuthoring.siteData : new SiteData();
            return data;
        }

        private SDASProjectData LoadFromJson()
        {
            return SDASJsonSerializer.LoadFromFile(jsonPath);
        }
    }
}

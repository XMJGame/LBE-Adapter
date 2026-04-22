using System.Collections.Generic;
using SDAS.Runtime.Core;
using UnityEngine;

namespace SDAS.Runtime.Mapping
{
    /// <summary>
    /// 运行时空间协调器：支持强绑定硬切与平滑校准两种重定向策略。
    /// </summary>
    public class SpaceCoordinator : MonoBehaviour, ISpaceCoordinator
    {
        [Header("Rig")]
        [SerializeField] private Transform xrRigOffset;
        [SerializeField] private Component xrOriginComponent;

        [Header("Redirection")]
        [SerializeField, Range(0.001f, 0.02f)] private float smoothLerp = 0.003f;
        [SerializeField] private bool defaultHardCutForLinkedChapter = true;

        private readonly Dictionary<int, ChapterNode> chapterLookup = new();
        private readonly Dictionary<int, ChapterMappingResult> mappingLookup = new();

        private IXrRigController rigController;
        private bool ghostModeEnabled;
        private Vector3 targetOffset;
        private float targetYaw;

        public void BindData(IReadOnlyList<ChapterNode> chapters, IReadOnlyList<ChapterMappingResult> mappings)
        {
            chapterLookup.Clear();
            mappingLookup.Clear();

            if (chapters != null)
            {
                for (var i = 0; i < chapters.Count; i++)
                {
                    chapterLookup[chapters[i].chapterId] = chapters[i];
                }
            }

            if (mappings != null)
            {
                for (var i = 0; i < mappings.Count; i++)
                {
                    mappingLookup[mappings[i].chapterId] = mappings[i];
                }
            }
        }

        public void ApplyChapter(int chapterId)
        {
            if (!mappingLookup.TryGetValue(chapterId, out var mapping))
            {
                Debug.LogWarning($"[SDAS] mapping not found for chapter {chapterId}");
                return;
            }

            var hardCut = defaultHardCutForLinkedChapter;
            if (chapterLookup.TryGetValue(chapterId, out var chapter))
            {
                hardCut = chapter.isLinkedToNext;
            }

            if (hardCut)
            {
                ApplyOffset(mapping.inverseOffset, mapping.inverseYaw);
                return;
            }

            targetOffset = mapping.inverseOffset;
            targetYaw = mapping.inverseYaw;
        }

        public void ApplyOffset(Vector3 inverseOffset, float inverseYaw)
        {
            EnsureRigController();
            rigController.PositionOffset = inverseOffset;
            rigController.YawOffset = inverseYaw;
            targetOffset = inverseOffset;
            targetYaw = inverseYaw;
        }

        public void SetGhostMode(bool enabled)
        {
            ghostModeEnabled = enabled;
        }

        private void Awake()
        {
            EnsureRigController();
            targetOffset = rigController.PositionOffset;
            targetYaw = rigController.YawOffset;
        }

        private void Update()
        {
            if (ghostModeEnabled)
            {
                return;
            }

            EnsureRigController();
            var currentPos = rigController.PositionOffset;
            var currentYaw = rigController.YawOffset;

            rigController.PositionOffset = Vector3.Lerp(currentPos, targetOffset, smoothLerp);
            rigController.YawOffset = Mathf.LerpAngle(currentYaw, targetYaw, smoothLerp);
        }

        private void EnsureRigController()
        {
            if (rigController != null)
            {
                return;
            }

            if (xrOriginComponent != null)
            {
                rigController = new XrOriginRigController(xrOriginComponent);
                return;
            }

            rigController = new TransformXrRigController(xrRigOffset != null ? xrRigOffset : transform);
        }
    }
}

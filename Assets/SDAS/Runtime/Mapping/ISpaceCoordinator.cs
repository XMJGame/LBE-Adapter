using UnityEngine;

namespace SDAS.Runtime.Mapping
{
    public interface ISpaceCoordinator
    {
        void ApplyChapter(int chapterId);
        void ApplyOffset(Vector3 inverseOffset, float inverseYaw);
        void SetGhostMode(bool enabled);
    }
}

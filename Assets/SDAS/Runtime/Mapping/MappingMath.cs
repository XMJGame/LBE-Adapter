using SDAS.Runtime.Core;
using UnityEngine;

namespace SDAS.Runtime.Mapping
{
    public static class MappingMath
    {
        /// <summary>
        /// 由章节世界摆放结果计算给 XR Rig 的逆向补偿。
        /// 当前版本只处理 Yaw 旋转，符合 XZ 平面适配假设。
        /// </summary>
        public static ChapterMappingResult BuildInverse(ChapterMappingResult chapterTransform)
        {
            var result = new ChapterMappingResult
            {
                chapterId = chapterTransform.chapterId,
                worldPos = chapterTransform.worldPos,
                worldRotEuler = chapterTransform.worldRotEuler
            };

            var yaw = chapterTransform.worldRotEuler.y;
            var inverseRot = Quaternion.Euler(0f, -yaw, 0f);
            result.inverseOffset = inverseRot * (-chapterTransform.worldPos);
            result.inverseYaw = -yaw;
            return result;
        }
    }
}

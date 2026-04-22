using SDAS.Runtime.Core;
using UnityEngine;

namespace SDAS.Runtime.Authoring
{
    /// <summary>
    /// 章节编辑载体：将 ChapterNode 数据挂载到场景对象用于可视化编辑。
    /// </summary>
    public class ChapterAuthoring : MonoBehaviour
    {
        public ChapterNode chapter = new()
        {
            chapterId = 1,
            chapterName = "New Chapter"
        };

        public bool drawGizmos = true;

        private void OnDrawGizmos()
        {
            if (!drawGizmos || chapter?.walkingAreas == null)
            {
                return;
            }

            for (var i = 0; i < chapter.walkingAreas.Count; i++)
            {
                var area = chapter.walkingAreas[i];
                var color = i == 0 ? Color.green : Color.gray;
                Gizmos.color = color;

                if (area.shape == AreaShapeType.Circle)
                {
                    Gizmos.DrawWireSphere(area.center, area.radius);
                }
                else
                {
                    var points = area.vertices;
                    for (var p = 0; p < points.Count; p++)
                    {
                        var a = points[p];
                        var b = points[(p + 1) % points.Count];
                        Gizmos.DrawLine(a, b);
                    }
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(area.pivot, 0.05f);
            }
        }
    }
}

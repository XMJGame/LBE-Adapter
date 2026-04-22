using NUnit.Framework;
using SDAS.Runtime.Core;
using SDAS.Runtime.Mapping;
using UnityEngine;

namespace SDAS.Tests
{
    public class GeometryAndMappingTests
    {
        [Test]
        public void PointInPolygon_WorksForSquare()
        {
            var poly = new[]
            {
                new Vector2(0, 0),
                new Vector2(4, 0),
                new Vector2(4, 4),
                new Vector2(0, 4)
            };

            Assert.IsTrue(Geometry2D.IsPointInPolygon(new Vector2(2, 2), poly));
            Assert.IsFalse(Geometry2D.IsPointInPolygon(new Vector2(6, 2), poly));
        }

        [Test]
        public void BuildInverse_ProducesOppositeYawAndOffset()
        {
            var src = new ChapterMappingResult
            {
                chapterId = 1,
                worldPos = new Vector3(3f, 0f, 0f),
                worldRotEuler = new Vector3(0f, 90f, 0f)
            };

            var inv = MappingMath.BuildInverse(src);
            Assert.AreEqual(-90f, inv.inverseYaw, 0.001f);
            Assert.AreEqual(-3f, inv.inverseOffset.z, 0.001f);
        }
    }
}

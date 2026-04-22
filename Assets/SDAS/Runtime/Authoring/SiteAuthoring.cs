using SDAS.Runtime.Core;
using UnityEngine;

namespace SDAS.Runtime.Authoring
{
    /// <summary>
    /// 场地编辑载体：用于在 Scene 中编辑 boundary / obstacles / gateways。
    /// </summary>
    public class SiteAuthoring : MonoBehaviour
    {
        public SiteData siteData = new();
        public bool drawGizmos = true;

        private static readonly Color BoundaryColor = new(0.2f, 0.5f, 1f, 1f);
        private static readonly Color ObstacleColor = new(1f, 0.2f, 0.2f, 1f);

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            DrawBoundary();
            DrawObstacles();
            DrawGateways();
        }

        private void DrawBoundary()
        {
            if (siteData.boundary == null || siteData.boundary.Count < 2)
            {
                return;
            }

            Gizmos.color = BoundaryColor;
            for (var i = 0; i < siteData.boundary.Count; i++)
            {
                var a = siteData.boundary[i];
                var b = siteData.boundary[(i + 1) % siteData.boundary.Count];
                Gizmos.DrawLine(a, b);
            }
        }

        private void DrawObstacles()
        {
            if (siteData.obstacles == null)
            {
                return;
            }

            Gizmos.color = ObstacleColor;
            for (var i = 0; i < siteData.obstacles.Count; i++)
            {
                var poly = siteData.obstacles[i].polygon;
                if (poly == null || poly.Count < 2)
                {
                    continue;
                }

                for (var p = 0; p < poly.Count; p++)
                {
                    var a = poly[p];
                    var b = poly[(p + 1) % poly.Count];
                    Gizmos.DrawLine(a, b);
                }
            }
        }

        private void DrawGateways()
        {
            if (siteData.gateways == null)
            {
                return;
            }

            for (var i = 0; i < siteData.gateways.Count; i++)
            {
                var gateway = siteData.gateways[i];
                Gizmos.color = gateway.gatewayType == GatewayType.Entrance ? Color.cyan
                    : gateway.gatewayType == GatewayType.Exit ? Color.magenta
                    : Color.white;

                Gizmos.DrawSphere(gateway.position, 0.08f);
                Gizmos.DrawLine(gateway.position, gateway.position + gateway.forward.normalized * 0.35f);
            }
        }
    }
}

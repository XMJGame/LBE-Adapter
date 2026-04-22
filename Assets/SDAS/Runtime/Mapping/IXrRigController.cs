using UnityEngine;

namespace SDAS.Runtime.Mapping
{
    /// <summary>
    /// 对 XR Origin/CameraOffset 的最小抽象，方便在非 XR 环境调试。
    /// </summary>
    public interface IXrRigController
    {
        Vector3 PositionOffset { get; set; }
        float YawOffset { get; set; }
    }
}

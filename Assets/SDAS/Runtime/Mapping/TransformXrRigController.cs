using UnityEngine;

namespace SDAS.Runtime.Mapping
{
    /// <summary>
    /// 以普通 Transform 模拟 XR Rig 偏移控制（可替换成 XROrigin 适配器）。
    /// </summary>
    public class TransformXrRigController : IXrRigController
    {
        private readonly Transform rigOffset;

        public TransformXrRigController(Transform rigOffset)
        {
            this.rigOffset = rigOffset;
        }

        public Vector3 PositionOffset
        {
            get => rigOffset != null ? rigOffset.localPosition : Vector3.zero;
            set
            {
                if (rigOffset != null)
                {
                    rigOffset.localPosition = value;
                }
            }
        }

        public float YawOffset
        {
            get => rigOffset != null ? rigOffset.localEulerAngles.y : 0f;
            set
            {
                if (rigOffset != null)
                {
                    var euler = rigOffset.localEulerAngles;
                    euler.y = value;
                    rigOffset.localEulerAngles = euler;
                }
            }
        }
    }
}

using System;
using System.Reflection;
using UnityEngine;

namespace SDAS.Runtime.Mapping
{
    /// <summary>
    /// 通过反射适配 Unity XR CoreUtils 的 XROrigin，避免硬依赖程序集。
    /// </summary>
    public class XrOriginRigController : IXrRigController
    {
        private readonly Transform offsetTransform;

        public XrOriginRigController(Component xrOriginComponent)
        {
            offsetTransform = ResolveOffsetTransform(xrOriginComponent);
        }

        public Vector3 PositionOffset
        {
            get => offsetTransform != null ? offsetTransform.localPosition : Vector3.zero;
            set
            {
                if (offsetTransform != null)
                {
                    offsetTransform.localPosition = value;
                }
            }
        }

        public float YawOffset
        {
            get => offsetTransform != null ? offsetTransform.localEulerAngles.y : 0f;
            set
            {
                if (offsetTransform != null)
                {
                    var euler = offsetTransform.localEulerAngles;
                    euler.y = value;
                    offsetTransform.localEulerAngles = euler;
                }
            }
        }

        private static Transform ResolveOffsetTransform(Component xrOriginComponent)
        {
            if (xrOriginComponent == null)
            {
                return null;
            }

            try
            {
                var type = xrOriginComponent.GetType();
                var offsetProp = type.GetProperty("CameraFloorOffsetObject", BindingFlags.Public | BindingFlags.Instance)
                                 ?? type.GetProperty("CameraYOffsetObject", BindingFlags.Public | BindingFlags.Instance);

                if (offsetProp?.GetValue(xrOriginComponent) is GameObject offsetGo)
                {
                    return offsetGo.transform;
                }

                var originProp = type.GetProperty("Origin", BindingFlags.Public | BindingFlags.Instance)
                                ?? type.GetProperty("OriginTransform", BindingFlags.Public | BindingFlags.Instance);

                if (originProp?.GetValue(xrOriginComponent) is Transform originTransform)
                {
                    return originTransform;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SDAS] Failed to resolve XROrigin offset transform: {ex.Message}");
            }

            return xrOriginComponent.transform;
        }
    }
}

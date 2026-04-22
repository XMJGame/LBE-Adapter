#if UNITY_EDITOR
using SDAS.Runtime.Authoring;
using UnityEditor;
using UnityEngine;

namespace SDAS.Editor
{
    [CustomEditor(typeof(SiteAuthoring))]
    public class SiteAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty siteDataProp;

        private static readonly Color BoundaryColor = new(0.2f, 0.5f, 1f, 1f);
        private static readonly Color ObstacleColor = new(1f, 0.2f, 0.2f, 1f);

        private void OnEnable()
        {
            siteDataProp = serializedObject.FindProperty("siteData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            if (GUILayout.Button("Add Boundary Vertex"))
            {
                AddBoundaryVertex();
            }

            if (GUILayout.Button("Add Obstacle"))
            {
                AddObstacle();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            var siteAuthoring = (SiteAuthoring)target;

            DrawBoundaryHandles(siteAuthoring);
            DrawObstacleHandles(siteAuthoring);
        }

        private static void DrawBoundaryHandles(SiteAuthoring targetSite)
        {
            var boundary = targetSite.siteData.boundary;
            if (boundary == null)
            {
                return;
            }

            Handles.color = BoundaryColor;
            for (var i = 0; i < boundary.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                var next = Handles.PositionHandle(boundary[i], Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(targetSite, "Move Boundary Vertex");
                    boundary[i] = next;
                    EditorUtility.SetDirty(targetSite);
                }

                if (boundary.Count > 1)
                {
                    Handles.DrawAAPolyLine(5f, boundary[i], boundary[(i + 1) % boundary.Count]);
                }
            }
        }

        private static void DrawObstacleHandles(SiteAuthoring targetSite)
        {
            if (targetSite.siteData.obstacles == null)
            {
                return;
            }

            Handles.color = ObstacleColor;
            for (var o = 0; o < targetSite.siteData.obstacles.Count; o++)
            {
                var polygon = targetSite.siteData.obstacles[o].polygon;
                if (polygon == null)
                {
                    continue;
                }

                for (var i = 0; i < polygon.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    var next = Handles.PositionHandle(polygon[i], Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(targetSite, "Move Obstacle Vertex");
                        polygon[i] = next;
                        EditorUtility.SetDirty(targetSite);
                    }

                    if (polygon.Count > 1)
                    {
                        Handles.DrawLine(polygon[i], polygon[(i + 1) % polygon.Count]);
                    }
                }
            }
        }

        private void AddBoundaryVertex()
        {
            var boundaryProp = siteDataProp.FindPropertyRelative("boundary");
            var index = boundaryProp.arraySize;
            boundaryProp.InsertArrayElementAtIndex(index);
            var position = ((SiteAuthoring)target).transform.position;
            boundaryProp.GetArrayElementAtIndex(index).vector3Value = position + new Vector3(index, 0f, 0f);
        }

        private void AddObstacle()
        {
            var obstaclesProp = siteDataProp.FindPropertyRelative("obstacles");
            var index = obstaclesProp.arraySize;
            obstaclesProp.InsertArrayElementAtIndex(index);

            var obstacleProp = obstaclesProp.GetArrayElementAtIndex(index);
            obstacleProp.FindPropertyRelative("obstacleId").stringValue = $"Obstacle_{index}";

            var polygonProp = obstacleProp.FindPropertyRelative("polygon");
            polygonProp.ClearArray();
            var basePos = ((SiteAuthoring)target).transform.position + Vector3.right * (index + 1);
            for (var i = 0; i < 4; i++)
            {
                polygonProp.InsertArrayElementAtIndex(i);
            }

            polygonProp.GetArrayElementAtIndex(0).vector3Value = basePos + new Vector3(-0.25f, 0f, -0.25f);
            polygonProp.GetArrayElementAtIndex(1).vector3Value = basePos + new Vector3(0.25f, 0f, -0.25f);
            polygonProp.GetArrayElementAtIndex(2).vector3Value = basePos + new Vector3(0.25f, 0f, 0.25f);
            polygonProp.GetArrayElementAtIndex(3).vector3Value = basePos + new Vector3(-0.25f, 0f, 0.25f);
        }
    }
}
#endif

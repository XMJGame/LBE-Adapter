#if UNITY_EDITOR
using SDAS.Runtime.Authoring;
using SDAS.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace SDAS.Editor
{
    [CustomEditor(typeof(ChapterAuthoring))]
    public class ChapterAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty chapterProp;

        private static readonly Color ActiveAreaColor = Color.green;
        private static readonly Color InactiveAreaColor = Color.gray;

        private void OnEnable()
        {
            chapterProp = serializedObject.FindProperty("chapter");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            if (GUILayout.Button("Add Circle Area"))
            {
                AddWalkArea(AreaShapeType.Circle);
            }

            if (GUILayout.Button("Add Polygon Area"))
            {
                AddWalkArea(AreaShapeType.Polygon);
            }

            if (GUILayout.Button("Auto Compute Pivots"))
            {
                AutoComputePivots((ChapterAuthoring)target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            var authoring = (ChapterAuthoring)target;
            if (authoring.chapter?.walkingAreas == null)
            {
                return;
            }

            for (var i = 0; i < authoring.chapter.walkingAreas.Count; i++)
            {
                var area = authoring.chapter.walkingAreas[i];
                Handles.color = i == 0 ? ActiveAreaColor : InactiveAreaColor;

                if (area.shape == AreaShapeType.Circle)
                {
                    EditorGUI.BeginChangeCheck();
                    var newCenter = Handles.PositionHandle(area.center, Quaternion.identity);
                    var newRadiusHandle = Handles.Slider(area.center + Vector3.right * area.radius, Vector3.right);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(authoring, "Edit Circle Area");
                        area.center = newCenter;
                        area.radius = Mathf.Max(0.1f, Vector3.Distance(newCenter, newRadiusHandle));
                        EditorUtility.SetDirty(authoring);
                    }

                    Handles.DrawWireDisc(area.center, Vector3.up, area.radius);
                }
                else
                {
                    DrawPolygonHandles(authoring, area);
                }

                Handles.color = Color.yellow;
                EditorGUI.BeginChangeCheck();
                var newPivot = Handles.PositionHandle(area.pivot, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(authoring, "Edit Pivot");
                    area.pivot = newPivot;
                    EditorUtility.SetDirty(authoring);
                }
            }
        }

        private static void DrawPolygonHandles(ChapterAuthoring authoring, WalkArea area)
        {
            if (area.vertices == null)
            {
                area.vertices = new System.Collections.Generic.List<Vector3>();
            }

            for (var i = 0; i < area.vertices.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                var next = Handles.PositionHandle(area.vertices[i], Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(authoring, "Move Polygon Vertex");
                    area.vertices[i] = next;
                    EditorUtility.SetDirty(authoring);
                }

                if (area.vertices.Count > 1)
                {
                    Handles.DrawLine(area.vertices[i], area.vertices[(i + 1) % area.vertices.Count]);
                }
            }

            if (area.vertices.Count == 0 && Handles.Button(authoring.transform.position, Quaternion.identity, 0.1f, 0.1f, Handles.SphereHandleCap))
            {
                Undo.RecordObject(authoring, "Init Polygon Vertex");
                area.vertices.Add(authoring.transform.position);
                EditorUtility.SetDirty(authoring);
            }
        }

        private static void AutoComputePivots(ChapterAuthoring authoring)
        {
            Undo.RecordObject(authoring, "Auto Compute Pivots");
            for (var i = 0; i < authoring.chapter.walkingAreas.Count; i++)
            {
                var area = authoring.chapter.walkingAreas[i];
                if (area.shape == AreaShapeType.Circle)
                {
                    area.pivot = area.center;
                }
                else
                {
                    var center = Vector3.zero;
                    if (area.vertices != null && area.vertices.Count > 0)
                    {
                        for (var p = 0; p < area.vertices.Count; p++)
                        {
                            center += area.vertices[p];
                        }

                        center /= area.vertices.Count;
                    }

                    area.pivot = center;
                }
            }

            EditorUtility.SetDirty(authoring);
        }

        private void AddWalkArea(AreaShapeType shape)
        {
            var walkingAreasProp = chapterProp.FindPropertyRelative("walkingAreas");
            var index = walkingAreasProp.arraySize;
            walkingAreasProp.InsertArrayElementAtIndex(index);

            var element = walkingAreasProp.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("shape").enumValueIndex = (int)shape;
            element.FindPropertyRelative("center").vector3Value = ((ChapterAuthoring)target).transform.position;
            element.FindPropertyRelative("radius").floatValue = 1f;
            element.FindPropertyRelative("forward").vector3Value = Vector3.forward;
            element.FindPropertyRelative("pivot").vector3Value = ((ChapterAuthoring)target).transform.position;
        }
    }
}
#endif

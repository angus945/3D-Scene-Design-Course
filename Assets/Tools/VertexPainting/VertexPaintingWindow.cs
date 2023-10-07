using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
//using VertexColorPainter.Editor;

public class VertexPaintingWindow : EditorWindow
{
    [MenuItem("Tools/Vertex Painting")]
    public static void ShowWindow()
    {
        VertexPaintingWindow window = GetWindow<VertexPaintingWindow>("Custom Window");
        window.Show();
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += SetSelectTarget;
        SceneView.duringSceneGui += OnSceneGUI;
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= SetSelectTarget;
        SceneView.duringSceneGui -= OnSceneGUI;

        selectedObject = null;
    }
    void OnGUI()
    {
        EditorGUILayout.ObjectField("Painting Target", selectedObject, typeof(GameObject));
        EditorGUILayout.LabelField($"{filters.Count} Objects Detect");
    }


    GameObject selectedObject;
    List<MeshFilter> filters = new List<MeshFilter>();
    void SetSelectTarget(SceneView sceneView)
    {
        if (Selection.activeGameObject != selectedObject)
        {
            selectedObject = Selection.activeGameObject;

            if (selectedObject != null)
            {
                filters.Clear();
                filters.AddRange(selectedObject.GetComponentsInChildren<MeshFilter>());
            }
        }

        Repaint();
    }

    public void OnSceneGUI(SceneView sceneView)
    {
        if (SceneView.lastActiveSceneView == null) return;
        if (Selection.activeGameObject == null) return;

        GetSceneViewMeshRaycast(filters);
    }
    public void GetSceneViewMeshRaycast(List<MeshFilter> raycastFilters)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        float num = float.PositiveInfinity;
        Vector3 point = Vector3.zero;
        Vector3 normal = Vector3.up;
        foreach (MeshFilter meshFilter in raycastFilters)
        {
            Mesh sharedMesh = meshFilter.sharedMesh;
            RaycastHit hit;
            if (sharedMesh && RayHitRefrelection.IntersectRayMesh(ray, sharedMesh, meshFilter.transform.localToWorldMatrix, out hit) && hit.distance < num)
            {
                point = hit.point;
                num = hit.distance;
                normal = hit.normal;
            }
        }

        Handles.color = Color.red;
        Handles.DrawWireDisc(point, normal, 0.3f, 3);
        //Debug.LogError(point);
        //return point;
    }

}


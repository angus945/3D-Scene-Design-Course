using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VertexPaintObject))]
public class VertexPaintObjectEditor : Editor
{
    VertexPaintObject paintObject;
    List<VertexPaintData> paintDatas { get => paintObject.paintMeshes; }
    bool dataExist { get => paintObject.paintMeshes != null || paintObject.paintMeshes.Count != 0; }

    //Default
    void OnEnable()
    {
        paintObject = (VertexPaintObject)target;

        BuildPaintMesh();

        SceneView.duringSceneGui -= DuringSceneGUI;
        SceneView.duringSceneGui += DuringSceneGUI;
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    //Override
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Reload MeshFilter"))
        {
            ReloadMeshFilters();
        }
        if (GUILayout.Button("Build Paint Mesh"))
        {
            BuildPaintMesh();
        }
        if(GUILayout.Button("Disable Paint Mesh"))
        {
            LoadSouceMesh();
        }

        isPaintingMode = EditorGUILayout.Toggle("Painting Mode", isPaintingMode);
    }

    //Painting Mesh Data
    void ReloadMeshFilters()
    {
        if(dataExist)
        {
            LoadSouceMesh();
        }

        paintObject.paintMeshes = new List<VertexPaintData>();
        LoadMeshFilters(paintObject.transform);

        EditorUtility.SetDirty(target);

        SaveSorceMesh();

        void LoadMeshFilters(Transform checkObject)
        {
            if (checkObject.TryGetComponent(out MeshFilter filter))
            {
                paintDatas.Add(new VertexPaintData(filter));
            }

            foreach (Transform child in checkObject.transform)
            {
                LoadMeshFilters(child);
            }
        }
    }
    void BuildPaintMesh()
    {
        if (!dataExist) return;

        LoadSouceMesh();

        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];

            Mesh meshCopy = Mesh.Instantiate(paintData.sourceMesh) as Mesh;
            meshCopy.colors = paintData.vertexColors;
            paintData.meshFilter.sharedMesh = meshCopy;
            paintData.cacheVertices = meshCopy.vertices;

            EditorUtility.ClearDirty(paintDatas[i].meshFilter);
        }
    }

    void SaveSorceMesh()
    {
        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];
            paintData.sourceMesh = paintData.meshFilter.sharedMesh;

            EditorUtility.ClearDirty(paintData.meshFilter);
        }
    }
    void LoadSouceMesh()
    {
        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];
            paintData.meshFilter.sharedMesh = paintData.sourceMesh;

            EditorUtility.ClearDirty(paintData.meshFilter);
        }
    }

    //Vertex Painting
    bool isPaintingMode;
    VertexPaintBrush vertexBrush = new VertexPaintBrush(3);
    void DuringSceneGUI(SceneView sceneView)
    {
        if (!isPaintingMode) return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        SceneViewMeshRaycast();
        PaintingObject();

        DrawBrushHandle();
    }
    void SceneViewMeshRaycast()
    {
        if (!dataExist) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        float minDistance = float.PositiveInfinity;

        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];

            Mesh mesh = paintData.meshFilter.sharedMesh;
            bool rayHit = RayHitRefrelection.IntersectRayMesh(ray, mesh, paintData.localToWorldMatrix, out RaycastHit hit);

            if(rayHit && hit.distance < minDistance)
            {
                vertexBrush.point = hit.point;
                vertexBrush.normal = hit.normal;

                minDistance = hit.distance;
            }
        }

        if(minDistance == float.PositiveInfinity)
        {
            vertexBrush.point = Vector3.zero;
        }
    }
    void DrawBrushHandle()
    {
        if (vertexBrush.point == Vector3.zero) return;

        Debug.Log(vertexBrush.point);

        Handles.color = Color.red;
        Handles.DrawWireDisc(vertexBrush.point, vertexBrush.normal, vertexBrush.size, 3);
    }
    void PaintingObject()
    {
        if (vertexBrush.point == Vector3.zero) return;

        //if (Event.current.button == 0)

        EventType type = Event.current.type;
        int button = Event.current.button;
        bool shift = Event.current.shift;
        bool control = Event.current.control;
        bool alt = Event.current.alt;

        if(button == 0 && (type == EventType.MouseDown || type == EventType.MouseDrag))
        {
            vertexBrush.color = shift ? Color.red : Color.white;

            for (int i = 0; i < paintDatas.Count; i++)
            {
                VertexPaintData paintData = paintDatas[i];

                PaintingMesh(paintData, vertexBrush);
            }
        }


   
    }
    void PaintingMesh(VertexPaintData data, VertexPaintBrush brush)
    {
        for (int i = 0; i < data.cacheVertices.Length; i++)
        {
            Vector3 vertex = data.cacheVertices[i];
            Vector3 worldVertex = data.localToWorldMatrix.MultiplyPoint(vertex);
            float distance = Vector3.Distance(worldVertex, brush.point);

            if (distance < brush.size)
            {
                data.vertexColors[i] = brush.color;
            }
        }

        data.meshFilter.sharedMesh.colors = data.vertexColors;
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VertexPaintObject))]
public class VertexPaintObjectEditor : Editor
{
    VertexPaintObject paintObject;
    List<VertexPaintData> paintDatas { get => paintObject.paintMeshes; }
    bool dataExist { get => paintObject.paintMeshes != null || paintObject.paintMeshes.Count != 0; }

    Material vertexColorMaterial;

    //Default
    void OnEnable()
    {
        paintObject = (VertexPaintObject)target;

        BuildPaintMesh();

        SceneView.duringSceneGui -= DuringSceneGUI;
        SceneView.duringSceneGui += DuringSceneGUI;

        vertexColorMaterial = new Material(Shader.Find("VertexPaint/VertexColorShader"));
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    //InspectorGUI
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawButtons();
        DrawPaintingTool();

        overlayVertexColor = EditorGUILayout.Toggle("overlay vertex color", overlayVertexColor);
    }
    void DrawButtons()
    {
        if (GUILayout.Button("Reload MeshFilter"))
        {
            ReloadMeshFilters();
        }
        if (GUILayout.Button("Build Paint Mesh"))
        {
            BuildPaintMesh();
        }
        if (GUILayout.Button("Disable Paint Mesh"))
        {
            LoadSouceMesh();
        }
        if (GUILayout.Button("Clear Color"))
        {
            ClearColor();
        }
    }
    void DrawPaintingTool()
    {
        isPaintingMode = EditorGUILayout.Toggle("Painting Mode", isPaintingMode);

        vertexBrush.size = EditorGUILayout.FloatField("brush size", vertexBrush.size);
        vertexBrush.fade = EditorGUILayout.FloatField("brush fade", vertexBrush.fade);
        vertexBrush.intensity = EditorGUILayout.Slider("brush intensity", vertexBrush.intensity, 0, 1);

        string[] toolbarLabels = { "Layer 0", "Layer 1", "Layer 2", "Layer 3" };
        vertexBrush.paintLayer = GUILayout.Toolbar(vertexBrush.paintLayer, toolbarLabels);
    }

    //Painting Mesh Data
    void ReloadMeshFilters()
    {
        if (dataExist)
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
    void ClearColor()
    {
        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];

            paintData.vertexColors = new Color[paintData.vertexColors.Length];
            paintData.meshFilter.sharedMesh.colors = paintData.vertexColors;

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
    VertexPaintBrush vertexBrush = new VertexPaintBrush(3, 1, 1, 1);
    readonly Vector3[] layerColors = new Vector3[]
    {
        new Vector3(-1,-1,-1),
        new Vector3(1,0,0),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
    };

    void DuringSceneGUI(SceneView sceneView)
    {
        if (!isPaintingMode) return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Tools.current = Tool.None;

        SceneViewMeshRaycast();
        PaintingObject();

        DrawBrushHandle();

        VertexColorDisplay();
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

            if (rayHit && hit.distance < minDistance)
            {
                vertexBrush.point = hit.point;
                vertexBrush.normal = hit.normal;

                minDistance = hit.distance;
            }
        }

        if (minDistance == float.PositiveInfinity)
        {
            vertexBrush.point = Vector3.zero;
        }
    }
    void DrawBrushHandle()
    {
        if (vertexBrush.point == Vector3.zero) return;

        Handles.color = new Color(1, 0, 0, vertexBrush.intensity);
        Handles.DrawWireDisc(vertexBrush.point, vertexBrush.normal, vertexBrush.size + vertexBrush.fade, 3);

        Handles.color = new Color(0, 0, 1, vertexBrush.intensity);
        Handles.DrawWireDisc(vertexBrush.point, vertexBrush.normal, vertexBrush.size, 3);
    }
    void PaintingObject()
    {
        if (vertexBrush.point == Vector3.zero) return;

        Event e = Event.current;

        EventType type = e.type;
        int button = e.button;
        bool shift = e.shift;
        bool control = e.control;
        bool alt = e.alt;

        if (button == 0 && (type == EventType.MouseDown || type == EventType.MouseDrag) && !alt)
        {
            vertexBrush.erase = shift;

            for (int i = 0; i < paintDatas.Count; i++)
            {
                VertexPaintData paintData = paintDatas[i];

                PaintingMesh(paintData, vertexBrush);
            }
        }

        if (type == EventType.ScrollWheel)
        {
            float delta = e.delta.y * -0.1f;

            if (control)
            {
                vertexBrush.size = Mathf.Max(0, vertexBrush.size + delta);
                e.Use();
                Repaint();
            }
            if (shift)
            {
                vertexBrush.fade = Mathf.Max(0, vertexBrush.fade + delta);
                e.Use();
                Repaint();
            }
            if (alt)
            {
                vertexBrush.intensity = Mathf.Clamp01(vertexBrush.intensity + delta * 0.1f);
                e.Use();
                Repaint();
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

            if (distance > brush.size + brush.fade) continue;

            Vector3 brushColor = layerColors[brush.paintLayer];
            Vector3 sourceColor = new Vector3(data.vertexColors[i].r, data.vertexColors[i].g, data.vertexColors[i].b);
            Vector3 maskedSouceColor = new(brushColor.x * sourceColor.x, brushColor.y * sourceColor.y, brushColor.z * sourceColor.z);
            Vector3 inverseBrushColor = Vector3.one - brushColor;
            Vector3 inverseMaskedSourceColor = new Vector3(inverseBrushColor.x * sourceColor.x, inverseBrushColor.y * sourceColor.y, inverseBrushColor.z * sourceColor.z);

            distance = distance - brush.size;
            distance = distance / brush.fade;
            distance = Mathf.Clamp01(distance);

            float intensity = (1 - distance) * brush.intensity;
            float eraseIntensity = (distance);

            Vector3 overlayColor, baseColor;
            if (brush.erase)
            {
                overlayColor = Vector3.Min(maskedSouceColor, brushColor * eraseIntensity);
                baseColor = inverseMaskedSourceColor;
            }
            else
            {
                overlayColor = Vector3.Max(maskedSouceColor, brushColor * intensity);
                baseColor = Vector3.Min(inverseMaskedSourceColor, inverseBrushColor * eraseIntensity);
            }

            Vector3 resultColor = baseColor + overlayColor;
            data.vertexColors[i] = new Color(resultColor.x, resultColor.y, resultColor.z);
        }

        data.meshFilter.sharedMesh.colors = data.vertexColors;
    }

    bool overlayVertexColor;
    void VertexColorDisplay()
    {
        if (!overlayVertexColor) return;

        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];

            Mesh mesh = paintData.meshFilter.sharedMesh;
            Graphics.DrawMesh(mesh, paintData.localToWorldMatrix, vertexColorMaterial, 0);
            //TODO Àu¤Æ
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VertexPaintObject))]
public class VertexPaintObjectEditor : Editor
{
    VertexPaintObject paintObject;
    List<VertexPaintData> paintDatas { get => paintObject.paintMeshes; }
    bool dataExist { get => paintObject.paintMeshes != null || paintObject.paintMeshes.Count != 0; }

    bool isPaintingMode;

    //Default
    void OnEnable()
    {
        if (Application.isPlaying) return;

        paintObject = (VertexPaintObject)target;

        BuildPaintMesh();

        SceneView.duringSceneGui -= DuringSceneGUI;
        SceneView.duringSceneGui += DuringSceneGUI;

        vertexColorMaterial = new Material(Shader.Find("VertexPaint/VertexColorShader"));
    }
    void OnDisable()
    {
        if (Application.isPlaying) return;

        SceneView.duringSceneGui -= DuringSceneGUI;

        //LoadSouceMesh();
    }

    //InspectorGUI
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        DrawMeshOption();
        DrawPaintingTool();
        //DrawMaterialProperity();
        DrawBrushProperity();

        DrawDebugOption();
    }

    void DrawMeshOption()
    {
        GUILayout.Label("Paint Mesh", EditorStyles.boldLabel);

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

        ClearMeshFilterDirty();
    }
    void DrawPaintingTool()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Vertex Painting", EditorStyles.boldLabel);

        string modeButton = isPaintingMode ? "Disable Painting" : "Enable Painting";
        GUI.color = isPaintingMode ? Color.green : Color.white;
        if (GUILayout.Button(modeButton))
        {
            isPaintingMode = !isPaintingMode;
        }
        GUI.color = Color.white;

        string[] toolbarLabels = { "Layer 0", "Layer 1", "Layer 2" };
        Vector3[] channels = { Vector3.one, new Vector3(1, 0, 0), new Vector3(0, 1, 0) };
        vertexBrush.paintLayer = GUILayout.Toolbar(vertexBrush.paintLayer, toolbarLabels);
        vertexColorMaterial.SetVector("_Display", channels[vertexBrush.paintLayer]);

    }
    void DrawMaterialProperity()
    {

    }
    void DrawBrushProperity()
    {
        if (!isPaintingMode) return;

        EditorGUILayout.Space();
        GUILayout.Label("Brush Properity", EditorStyles.boldLabel);
        vertexBrush.size = EditorGUILayout.FloatField("brush size", vertexBrush.size);
        vertexBrush.fade = EditorGUILayout.FloatField("brush fade", vertexBrush.fade);
        vertexBrush.intensity = EditorGUILayout.Slider("brush intensity", vertexBrush.intensity, 0, 1);
    }
    void DrawDebugOption()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Debug Option", EditorStyles.boldLabel);

        string displayMode = "Display: " + (overlayVertexColor ? "Vertex Color" : "Material");
        GUI.color = overlayVertexColor ? Color.yellow : Color.white;
        if(GUILayout.Button(displayMode))
        {
            overlayVertexColor = !overlayVertexColor;
        }
        GUI.color =  Color.white;
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
    void ClearMeshFilterDirty()
    {
        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];

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
    VertexPaintBrush vertexBrush = new VertexPaintBrush(0, 1, 1, 1);
    readonly Vector3[] layerColors = new Vector3[]
    {
        new Vector3(-1,-1,-1),
        new Vector3(1,0,0),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
    };

    void DuringSceneGUI(SceneView sceneView)
    {
        if (isPaintingMode)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Tools.current = Tool.None;

            SceneViewMeshRaycast();
            PaintingObject();

            DrawBrushHandle();
            DrawVertex();
        }

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

        PaintHotKey(e, type, shift, control, alt);
    }
    void PaintHotKey(Event e, EventType type, bool shift, bool control, bool alt)
    {
        if (type == EventType.ScrollWheel)
        {
            float delta = e.delta.y * -0.05f;

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
                vertexBrush.intensity = Mathf.Clamp01(vertexBrush.intensity + delta);
                e.Use();
                Repaint();
            }
        }
    }
    void PaintingMesh(VertexPaintData data, VertexPaintBrush brush)
    {
        for (int i = 0; i < data.cacheVertices.Length; i++)
        {
            float intensity, eraseIntensity;
            if (BrushIntensity(data, brush, i, out intensity, out eraseIntensity)) continue;

            Vector3 sourceColor, paintLayer, paintSource, paintCull;
            ColorLayers(data, brush, i, out sourceColor, out paintLayer, out paintSource, out paintCull);

            if (brush.paintLayer > 0)
            {
                Vector3 paintColor, baseColor;
                if (!brush.erase)
                {
                    paintColor = Vector3.Max(paintSource, paintLayer * intensity);
                    baseColor = paintCull;
                }
                else
                {
                    paintColor = Vector3.Min(paintSource, paintLayer * eraseIntensity);
                    baseColor = paintCull;
                }

                Vector3 resultColor = baseColor + paintColor;
                data.vertexColors[i] = new Color(resultColor.x, resultColor.y, resultColor.z);
            }
            else
            {
                Vector3 resultColor = Vector3.Min(sourceColor, Vector3.one * eraseIntensity);
                data.vertexColors[i] = new Color(resultColor.x, resultColor.y, resultColor.z);
            }

        }

        data.meshFilter.sharedMesh.colors = data.vertexColors;
        EditorUtility.SetDirty(target);
    }
    bool BrushIntensity(VertexPaintData data, VertexPaintBrush brush, int index, out float intensity, out float eraseIntensity)
    {
        Vector3 vertex = data.cacheVertices[index];
        Vector3 worldVertex = data.localToWorldMatrix.MultiplyPoint(vertex);
        float distance = Vector3.Distance(worldVertex, brush.point);

        distance = distance - brush.size;
        distance = distance / brush.fade;
        distance = Mathf.Clamp01(distance);

        intensity = (1 - distance) * brush.intensity;
        eraseIntensity = (distance) * brush.intensity;

        return (distance > brush.size + brush.fade);
    }
    void ColorLayers(VertexPaintData data, VertexPaintBrush brush, int index, out Vector3 sourceColor, out Vector3 paintLayer, out Vector3 paintSource, out Vector3 paintCull)
    {
        paintLayer = layerColors[brush.paintLayer];
        Vector3 cullLayer = Vector3.one - paintLayer;

        sourceColor = new Vector3(data.vertexColors[index].r, data.vertexColors[index].g, data.vertexColors[index].b);

        paintSource = new Vector3(sourceColor.x * paintLayer.x, sourceColor.y * paintLayer.y, sourceColor.z * paintLayer.z);
        paintCull = new Vector3(sourceColor.x * cullLayer.x, sourceColor.y * cullLayer.y, sourceColor.z * cullLayer.z);
    }
    void PaintingMesh_Erase(VertexPaintData data, VertexPaintBrush brush)
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
    Material vertexColorMaterial;
    void DrawBrushHandle()
    {
        if (vertexBrush.point == Vector3.zero) return;

        Handles.color = new Color(1, 0, 0, vertexBrush.intensity);
        Handles.DrawWireDisc(vertexBrush.point, vertexBrush.normal, vertexBrush.size + vertexBrush.fade, 3);

        Handles.color = new Color(0, 0, 1, vertexBrush.intensity);
        Handles.DrawWireDisc(vertexBrush.point, vertexBrush.normal, vertexBrush.size, 3);
    }
    void DrawVertex()
    {
        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];

            for (int vi = 0; vi < paintData.cacheVertices.Length; vi++)
            {
                Vector3 vertex = paintData.cacheVertices[vi];
                Vector3 position = paintData.localToWorldMatrix.MultiplyPoint(vertex);
                float distance = Vector3.Distance(position, vertexBrush.point);
                bool inRange = distance < vertexBrush.size + vertexBrush.fade;

                Handles.color = inRange ? Color.blue : Color.gray;
                Handles.DrawWireCube(position, Vector3.one * 0.1f);
            }
        }
    }
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

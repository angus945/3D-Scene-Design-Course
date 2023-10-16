using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CacheData
{
    public int cacheIndex;
    public MeshFilter filter;
    public Matrix4x4 transform;
    public int[] maskIndices;

    public CacheData(int cacheIndex, MeshFilter filter, Matrix4x4 transform, int[] maskIndices)
    {
        this.cacheIndex = cacheIndex;
        this.filter = filter;
        this.transform = transform;
        this.maskIndices = maskIndices;
    }
}
public class MeshLayer
{
    public int count { get => cacheDatas.Count; }
    public List<CacheData> cacheDatas = new List<CacheData>();

    public void Add(int filterIndex, MeshFilter filter, Matrix4x4 transform, int[] maskedIndices)
    {
        cacheDatas.Add(new CacheData(filterIndex, filter, transform, maskedIndices));
    }
}
public class PaintingLayers
{
    List<Material> materials = new List<Material>();
    Dictionary<Material, MeshLayer> meshLayers = new Dictionary<Material, MeshLayer>();
    public List<Material> Materials { get => materials; }

    public void AddFilter(int cacheIndex, MeshFilter filter)
    {
        Material[] materials = filter.GetComponent<MeshRenderer>().sharedMaterials;
        for (int mi = 0; mi < materials.Length; mi++)
        {
            Material material = materials[mi];
            Mesh mesh = filter.sharedMesh;
            int[] indices = mesh.GetIndices(mi);

            AddLayer(cacheIndex, material, filter, indices);
        }
    }
    void AddLayer(int cacheIndex, Material material, MeshFilter filter, int[] indices)
    {
        if (!meshLayers.ContainsKey(material))
        {
            meshLayers.Add(material, new MeshLayer());
            materials.Add(material);
        }

        meshLayers[material].Add(cacheIndex, filter, filter.transform.localToWorldMatrix, indices);
    }

    public MeshLayer GetLayer(int index)
    {
        return meshLayers[materials[index]];
    }
    public MeshLayer GetLayer(Material material)
    {
        return meshLayers[material];
    }

    public void InspectorGUI_Display()
    {
        for (int i = 0; i < materials.Count; i++)
        {
            EditorGUILayout.LabelField($"layer {i} : {materials[i].name}");
        }
    }

    public void SceneGUI_DrawVertex()
    {
        for (int i = 0; i < materials.Count; i++)
        {
            SceneGUI_DrawVertex(i);
        }
    }
    public void SceneGUI_DrawVertex(int layerIndex)
    {
        MeshLayer layer = meshLayers[materials[layerIndex]];
        for (int i = 0; i < layer.count; i++)
        {

            Mesh mesh = layer.cacheDatas[i].filter.sharedMesh;
            Matrix4x4 transform = layer.cacheDatas[i].transform;
            int[] indices = layer.cacheDatas[i].maskIndices;

            for (int vi = 0; vi < indices.Length; vi++)
            {
                int index = indices[vi];
                Vector3 vertex = mesh.vertices[index];
                Vector3 position = transform.MultiplyPoint(vertex);

                //TODO Range Color
                Handles.color = Color.white;
                Handles.DrawWireCube(position, Vector3.one * 0.1f);
            }
        }
    }
}

public class VertexPainterBrush
{
    public float size;
    public float fade;
    public float intensity;
    public bool erase;

    public int paintChannel;
    public Vector3 paintChannelColor { get => channelColors[paintChannel]; }
    readonly Vector3[] channelColors = new Vector3[]
    {
            new Vector3(-1,-1,-1),
            new Vector3(1,0,0),
            new Vector3(0,1,0),
            new Vector3(0,0,1),
    };

    public int paintMaterial;

    public Vector3 position = Vector3.zero;
    public Vector3 normal = Vector3.up;

    public VertexPainterBrush(float size, float fade, float intensity, int paintLayer)
    {
        this.size = size;
        this.fade = fade;
        this.intensity = intensity;
        this.paintMaterial = paintLayer;
    }

    public void InspectirGUI_DrawBrushProperity(Material[] materials)
    {
        EditorGUILayout.Space();
        GUILayout.Label("Brush Properity", EditorStyles.boldLabel);

        string[] channelLabels = { "Clear", "Channel R", "Channel G", "Channel B" };
        paintChannel = GUILayout.Toolbar(paintChannel, channelLabels);

        string[] materialLabels = Array.ConvertAll<Material, string>(materials, n => $"Material Mask: {n.name}");
        paintMaterial = EditorGUILayout.Popup(paintMaterial, materialLabels);

        EditorGUILayout.Space();
        EditorGUILayout.Vector3Field("position", position);
        size = EditorGUILayout.FloatField("brush size", size);
        fade = EditorGUILayout.FloatField("brush fade", fade);
        intensity = EditorGUILayout.Slider("brush intensity", intensity, 0, 1);
        //vertexColorMaterial.SetVector("_Display", channels[vertexBrush.paintLayer]);
    }
    public void SceneGUI_DrawBrushHandle()
    {
        Handles.color = new Color(1, 0, 0, intensity);
        Handles.DrawWireDisc(position, normal, size + fade, 3);

        Handles.color = new Color(0, 0, 1, intensity);
        Handles.DrawWireDisc(position, normal, size, 3);
    }
    public void SceneGUI_BrushHotkey(Action repaint)
    {
        erase = Event.current.shift;

        if (Event.current.type == EventType.ScrollWheel)
        {
            Event e = Event.current;
            float delta = e.delta.y * -0.05f;

            if (e.control)
            {
                size = Mathf.Max(0, size + delta);
                e.Use();
                repaint.Invoke();
            }
            if (e.shift)
            {
                fade = Mathf.Max(0, fade + delta);
                e.Use();
                repaint.Invoke();
            }
            if (e.alt)
            {
                intensity = Mathf.Clamp01(intensity + delta);
                e.Use();
                repaint.Invoke();
            }
        }
    }

    public bool BrushIntensity(Vector3 target, out float intensity, out float eraseIntensity)
    {
        float distance = Vector3.Distance(position, target);

        distance = distance - size;
        distance = distance / fade;
        distance = Mathf.Clamp01(distance);

        intensity = (1 - distance) * this.intensity;
        eraseIntensity = (distance) * this.intensity;

        return (distance <= size + fade);
    }
}
public class VertexPainter
{
    public bool isPainting { get; private set; }

    public void InspectorGUI_EnablePainting()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Vertex Painting", EditorStyles.boldLabel);

        string label = isPainting ? "Painting Mode: ON" : "Painting Mode: OFF";
        GUI.color = isPainting ? Color.green : Color.white;
        if (GUILayout.Button(label))
        {
            isPainting = !isPainting;
        }
        GUI.color = Color.white;
    }
    public void SceneGUI_Raycast(MeshLayer layer, out Vector3 position, out Vector3 normal)
    {
        position = Vector3.zero;
        normal = Vector3.up;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        float minDistance = float.PositiveInfinity;

        for (int i = 0; i < layer.cacheDatas.Count; i++)
        {
            MeshFilter filter = layer.cacheDatas[i].filter;
            Mesh mesh = filter.sharedMesh;
            Matrix4x4 transform = layer.cacheDatas[i].transform;
            bool rayHit = RayHitRefrelection.IntersectRayMesh(ray, mesh, transform, out RaycastHit hit);

            if (rayHit && hit.distance < minDistance)
            {
                position = hit.point;
                normal = hit.normal;

                minDistance = hit.distance;
            }
        }

        if (minDistance == float.PositiveInfinity)
        {
            position = Vector3.zero;
        }
    }
    public bool SceneGUI_Paint(List<VertexPaintData> paintDatas, MeshLayer layer, VertexPainterBrush brush)
    {
        //if (vertexBrush.point == Vector3.zero) return;
        if (Event.current.button != 0) return false;
        if (Event.current.type != EventType.MouseDown && Event.current.type != EventType.MouseDrag) return false;
        if (Event.current.alt) return false;

        for (int i = 0; i < layer.count; i++)
        {
            CacheData cacheData = layer.cacheDatas[i];

            PaintingMesh(paintDatas, cacheData, brush);
        }

        return true;
    }
    void PaintingMesh(List<VertexPaintData> paintDatas, CacheData cacheData, VertexPainterBrush brush)
    {
        foreach (int index in cacheData.maskIndices)
        {
            Vector3 vertex = cacheData.filter.sharedMesh.vertices[index];
            Vector3 position = cacheData.transform.MultiplyPoint(vertex);

            float intensity, eraseIntensity;
            if (!brush.BrushIntensity(position, out intensity, out eraseIntensity)) continue;

            Vector3 sourceColor, paintLayer, paintSource, paintCull;
            VertexPaintData data = paintDatas[cacheData.cacheIndex];
            ColorLayers(data, brush, index, out sourceColor, out paintLayer, out paintSource, out paintCull);

            if (brush.paintChannel > 0)
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
                data.vertexColors[index] = new Color(resultColor.x, resultColor.y, resultColor.z);
            }
            else
            {
                Vector3 resultColor = Vector3.Min(sourceColor, Vector3.one * eraseIntensity);
                data.vertexColors[index] = new Color(resultColor.x, resultColor.y, resultColor.z);
            }

            data.meshFilter.sharedMesh.colors = data.vertexColors;
        }

    }
    void ColorLayers(VertexPaintData data, VertexPainterBrush brush, int index, out Vector3 sourceColor, out Vector3 paintLayer, out Vector3 paintSource, out Vector3 paintCull)
    {
        //paintLayer = new Vector3(1, 0, 0);
        paintLayer = brush.paintChannelColor;
        Vector3 cullLayer = Vector3.one - paintLayer;

        sourceColor = new Vector3(data.vertexColors[index].r, data.vertexColors[index].g, data.vertexColors[index].b);

        paintSource = new Vector3(sourceColor.x * paintLayer.x, sourceColor.y * paintLayer.y, sourceColor.z * paintLayer.z);
        paintCull = new Vector3(sourceColor.x * cullLayer.x, sourceColor.y * cullLayer.y, sourceColor.z * cullLayer.z);
    }
}

public class VertexColorDebug
{
    bool overlayVertexColor;
    Material debugMaterial;

    public void InspectorGUI_DrawDisplayButton()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Debug Option", EditorStyles.boldLabel);

        string displayMode = "Display: " + (overlayVertexColor ? "Vertex Color" : "Material");
        GUI.color = overlayVertexColor ? Color.yellow : Color.white;
        if (GUILayout.Button(displayMode))
        {
            overlayVertexColor = !overlayVertexColor;
            SceneView.RepaintAll();
        }
        GUI.color = Color.white;
    }
    public void SceneGUI_VertexColorDisplay(List<VertexPaintData> paintDatas)
    {
        if (!overlayVertexColor) return;

        RenderParams rp = new RenderParams(debugMaterial);

        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];

            Mesh mesh = paintData.meshFilter.sharedMesh;
            Matrix4x4 transform = paintData.meshFilter.transform.localToWorldMatrix;

            for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                Graphics.RenderMesh(rp, mesh, submesh, transform);
            }
        }
    }

    public void SetMaterial(Shader shader)
    {
        debugMaterial = new Material(shader);
    }
}

[CustomEditor(typeof(VertexPaintObject))]
public class VertexPaintObjectEditor : Editor
{
    VertexPaintObject paintObject;
    List<VertexPaintData> paintDatas { get => paintObject.paintMeshes; }

    PaintingLayers layers = new PaintingLayers();
    VertexPainter painter = new VertexPainter();
    VertexPainterBrush brush = new VertexPainterBrush(0.1f, 0.9f, 1, 1);
    VertexColorDebug debug = new VertexColorDebug();

    //Default
    void OnEnable()
    {
        if (Application.isPlaying) return;

        paintObject = (VertexPaintObject)target;

        CacheVertices();
        BuildPaintMesh();

        debug.SetMaterial(Shader.Find("VertexPaint/VertexColorShader"));

        SceneView.duringSceneGui -= DuringSceneGUI;
        SceneView.duringSceneGui += DuringSceneGUI;
    }
    void OnDisable()
    {
        if (Application.isPlaying) return;

        SceneView.duringSceneGui -= DuringSceneGUI;
    }
    void CacheVertices()
    {
        if (paintDatas == null) return;

        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];
            layers.AddFilter(i, paintData.meshFilter);
        }
    }

    //InspectorGUI
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour(paintObject), typeof(VertexPaintObject), false);
        GUI.enabled = true;

        DrawMeshOption();

        //layers.InspectorGUI_Display();

        painter.InspectorGUI_EnablePainting();

        if(painter.isPainting)
        {
            brush.InspectirGUI_DrawBrushProperity(layers.Materials.ToArray());
        }

        debug.InspectorGUI_DrawDisplayButton();
    }

    void DrawMeshOption()
    {
        GUILayout.Label("Paint Mesh", EditorStyles.boldLabel);

        if (GUILayout.Button("Reload MeshFilter"))
        {
            ResetToSourceMesh();

            paintDatas.Clear();
            LoadMeshFilters(paintObject.transform);
        }
        if (GUILayout.Button("Build Paint Mesh"))
        {
            BuildPaintMesh();
            CacheVertices();
        }
        if (GUILayout.Button("Clear Color"))
        {
            ResetVertexColor();
        }

        //ClearMeshFilterDirty();
    }
    void BuildPaintMesh()
    {
        if (paintDatas == null) return;

        ResetToSourceMesh();

        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];

            Mesh meshCopy = Mesh.Instantiate(paintData.sourceMesh) as Mesh;
            meshCopy.colors = paintData.vertexColors;
            paintData.meshFilter.sharedMesh = meshCopy;

            EditorUtility.ClearDirty(paintDatas[i].meshFilter);
        }
    }
    void ResetVertexColor()
    {
        if (paintDatas == null) return;

        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];

            paintData.vertexColors = new Color[paintData.vertexCount];
            paintData.meshFilter.sharedMesh.colors = paintData.vertexColors;

            EditorUtility.ClearDirty(paintDatas[i].meshFilter);
        }
    }
    void ResetToSourceMesh()
    {
        if (paintDatas == null) return;

        for (int i = 0; i < paintDatas.Count; i++)
        {
            VertexPaintData paintData = paintDatas[i];
            paintData.meshFilter.sharedMesh = paintData.sourceMesh;

            EditorUtility.ClearDirty(paintData.meshFilter);
        }
    }

    void LoadMeshFilters(Transform checkObject)
    {
        if (checkObject.TryGetComponent(out MeshFilter filter))
        {
            paintObject.paintMeshes.Add(new VertexPaintData(filter, filter.sharedMesh));
        }

        foreach (Transform child in checkObject.transform)
        {
            LoadMeshFilters(child);
        }
    }

    //Vertex Painting
    void DuringSceneGUI(SceneView sceneView)
    {
        debug.SceneGUI_VertexColorDisplay(paintDatas);

        if (!painter.isPainting) return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Tools.current = Tool.None;

        brush.SceneGUI_BrushHotkey(Repaint);
        brush.SceneGUI_DrawBrushHandle();

        int layerIndex = brush.paintMaterial;
        MeshLayer layer = layers.GetLayer(layerIndex);
        painter.SceneGUI_Raycast(layer, out brush.position, out brush.normal);

        if (painter.SceneGUI_Paint(paintDatas, layer, brush))
        {
            EditorUtility.SetDirty(target);
            for (int i = 0; i < paintDatas.Count; i++)
            {
                EditorUtility.ClearDirty(paintDatas[i].meshFilter);
            }
        }

        layers.SceneGUI_DrawVertex(layerIndex);
    }
}

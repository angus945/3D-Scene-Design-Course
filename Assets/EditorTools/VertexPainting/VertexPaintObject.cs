using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VertexPaintData
{
    public MeshFilter meshFilter;
    public Matrix4x4 localToWorldMatrix { get => meshFilter.transform.localToWorldMatrix; }
    public Vector3[] cacheVertices;

    public Mesh sourceMesh;
    public Color[] vertexColors;

    public VertexPaintData(MeshFilter filter)
    {
        this.meshFilter = filter;
        vertexColors = new Color[filter.sharedMesh.vertexCount];
    }

    public void ApplyVertexColor()
    {
        Mesh instanceMesh = Mesh.Instantiate(sourceMesh) as Mesh;
        instanceMesh.colors = vertexColors;

        meshFilter.mesh = instanceMesh;
    }
}

public class VertexPaintObject : MonoBehaviour
{
    public List<VertexPaintData> paintMeshes;

    void Awake()
    {
        for (int i = 0; i < paintMeshes.Count; i++)
        {
            paintMeshes[i].ApplyVertexColor();
        }

        Destroy(this);
    }
}

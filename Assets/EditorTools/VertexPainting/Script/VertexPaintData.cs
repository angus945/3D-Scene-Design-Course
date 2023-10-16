using System;
using UnityEngine;

[System.Serializable]
public class VertexPaintData
{
    public Mesh sourceMesh;
    public MeshFilter meshFilter;
    public Color[] vertexColors;
    public int vertexCount { get => sourceMesh.vertexCount; }

    public VertexPaintData(MeshFilter filter, Mesh source)
    {
        this.meshFilter = filter;
        this.sourceMesh = source;
        vertexColors = new Color[filter.sharedMesh.vertexCount];
    }

    public void ApplyVertexColor()
    {
        Mesh instanceMesh = Mesh.Instantiate(sourceMesh) as Mesh;
        instanceMesh.colors = vertexColors;
        Debug.Log(instanceMesh.colors.Length);

        meshFilter.mesh = instanceMesh;
    }

    public void ResetSharedMesh()
    {
        meshFilter.sharedMesh = sourceMesh;
    }
}

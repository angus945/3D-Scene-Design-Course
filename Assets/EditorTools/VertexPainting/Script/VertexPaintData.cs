using System;
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
        Debug.Log(instanceMesh.colors.Length);

        meshFilter.mesh = instanceMesh;
    }

    public void ResetSharedMesh()
    {
        meshFilter.sharedMesh = sourceMesh;
    }
}

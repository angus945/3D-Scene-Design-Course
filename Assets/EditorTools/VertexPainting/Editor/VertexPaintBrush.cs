using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VertexPaintMode
{
    Max,
    Add,
    Set,
}
public class VertexPaintBrush
{
    public float size;
    public float fade;
    public float intensity;
    public bool erase;

    public int paintLayer;

    public Vector3 point;
    public Vector3 normal;

    public VertexPaintBrush(float size, float fade, float intensity, int paintLayer)
    {
        this.size = size;
        this.fade = fade;
        this.intensity = intensity;
        this.paintLayer = paintLayer;
    }
}

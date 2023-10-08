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
    public Color color;

    public Vector3 point;
    public Vector3 normal;

    public VertexPaintBrush(float size)
    {
        this.size = size;
    }
}

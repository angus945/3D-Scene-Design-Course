using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VertexPaintObject : MonoBehaviour
{
    public List<VertexPaintData> paintMeshes = new List<VertexPaintData>();

    void Awake()
    {
        ApplyVertexColor();
    }
    void OnEnable()
    {
#if UNITY_EDITOR
        ResetMesh();
#endif
    }

    void ApplyVertexColor()
    {
        if (!Application.isPlaying) return;

        for (int i = 0; i < paintMeshes.Count; i++)
        {
            paintMeshes[i].ApplyVertexColor();
        }

        Destroy(this);
    }
    void ResetMesh()
    {
        if (Application.isPlaying) return;

        for (int i = 0; i < paintMeshes.Count; i++)
        {
            paintMeshes[i].ResetSharedMesh();
        }
    }

}

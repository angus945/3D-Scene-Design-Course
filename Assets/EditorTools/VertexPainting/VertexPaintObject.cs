using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

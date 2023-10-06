using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AlignObjects : EditorWindow
{

    [MenuItem("Buildings/AlignSelects")]
    static void AlignSelectObject()
    {
        GameObject[] selects = Selection.gameObjects;

        for (int i = 0; i < selects.Length; i++)
        {
            Transform parent = selects[i].transform;

            AlignObject(parent);
        }
    }
    static void AlignObject(Transform align)
    {
        Vector3 position = align.localPosition;
        Vector3 rotation = align.localRotation.eulerAngles;

        position.x = Mathf.Round(position.x * 2f) / 2;
        position.y = Mathf.Round(position.y * 2f) / 2;
        position.z = Mathf.Round(position.z * 2f) / 2;

        rotation.x = Mathf.Round(rotation.x / 90f) * 90;
        rotation.y = Mathf.Round(rotation.y / 90f) * 90;
        rotation.z = Mathf.Round(rotation.z / 90f) * 90;

        align.localPosition = position;
        align.localRotation = Quaternion.Euler(rotation);

        foreach (Transform child in align.transform)
        {
            AlignObject(child);
        }
    }
}

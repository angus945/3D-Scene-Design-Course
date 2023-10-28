using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class SkyboxRotator : MonoBehaviour
{
    [SerializeField] float speed;

    Material skybox;

    void LateUpdate()
    {
        if(skybox != RenderSettings.skybox)
        {
            skybox = RenderSettings.skybox;
        }

        skybox.SetFloat("_Rotation", speed * Time.time); ;
    }
}

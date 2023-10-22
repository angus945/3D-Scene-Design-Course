using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainBlending : MonoBehaviour
{
    [SerializeField] Terrain terrain;
    [SerializeField] int resolution = 1024;

    public RenderTexture terrainColorMap, terrainHeightMap;
    public Material blendMaterial;

    void Start()
    {
        BakeTerrainMaps();
    }
    void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;

        BakeTerrainMaps();
#endif

    }

    void CreateTextures()
    {
        if (terrainColorMap == null)
        {
            terrainColorMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.Default);
        }
        if(terrainHeightMap == null)
        {
            terrainHeightMap = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.Depth);
        }
    }
    void BakeTerrainMaps()
    {
        CreateTextures();

        Camera camera = GetComponent<Camera>();

        terrain.drawTreesAndFoliage = false;

        camera.targetTexture = terrainColorMap;
        camera.Render();

        camera.targetTexture = terrainHeightMap;
        camera.Render();

        terrain.drawTreesAndFoliage = true;

        Shader.SetGlobalTexture("_TerrainColorMap", terrainColorMap);
        Shader.SetGlobalTexture("_TerrainHeightMap", terrainHeightMap);
        Shader.SetGlobalVector("_MapResolution", Vector2.one * resolution);

        Shader.SetGlobalVector("_TerrainSize", new Vector4(terrain.terrainData.size.x, terrain.terrainData.size.z));

        Shader.SetGlobalFloat("_CameraHeight", GetComponent<Camera>().transform.position.y);
        Shader.SetGlobalFloat("_CameraFarPlane", GetComponent<Camera>().farClipPlane);
    }
}

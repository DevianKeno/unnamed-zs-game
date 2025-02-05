using UnityEngine;

public class TerrainHeightTexture : MonoBehaviour
{
    public Terrain terrain;
    public float grassHeight = 10f;
    public float rockHeight = 30f;
    public float snowHeight = 50f;
    public TerrainLayer[] terrainLayers; // Assign terrain layers in Inspector

    void Start()
    {
        ApplyTextures();
    }

    void ApplyTextures()
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;
        float[,,] splatmapData = new float[width, height, terrainLayers.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float worldX = (x / (float)width) * terrainData.size.x;
                float worldZ = (y / (float)height) * terrainData.size.z;
                float terrainHeight = terrainData.GetHeight(x, y);

                float[] textureWeights = new float[terrainLayers.Length];

                // Assign textures based on height
                if (terrainHeight < grassHeight)
                    textureWeights[0] = 1; // Grass
                else if (terrainHeight < rockHeight)
                    textureWeights[1] = 1; // Rock
                else
                    textureWeights[2] = 1; // Snow

                for (int i = 0; i < terrainLayers.Length; i++)
                {
                    splatmapData[x, y, i] = textureWeights[i];
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}

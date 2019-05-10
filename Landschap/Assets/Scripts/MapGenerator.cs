using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    
    const int mapChunkSize = 241;
    [Range(0, 6)]
    public int octaves;
    public int seed;
    public float noiseScale;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    [Range(0, 1)]
    public float persistance;
    public float lacuanrity;
    [Range(0, 6)]
    public int LevelOfDetail;
    public Vector2 offset;
    public TerrainType[] regions;

    void Start()
    {
        GenerateMap();
        
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale,seed, octaves,persistance, lacuanrity, offset);
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].heightValue)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }


        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        mapDisplay.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap,mapChunkSize,mapChunkSize));
        mapDisplay.DrawMesh(MeshGenerator.GenerateTerrain(noiseMap, meshHeightMultiplier, meshHeightCurve, LevelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
    }
	
}
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float heightValue;
    public Color colour;
}

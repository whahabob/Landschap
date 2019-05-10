using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public int mapWidth, mapHeight;
    [Range(0, 6)]
    public int octaves;
    public int seed;
    public float noiseScale;
    [Range(0, 1)]
    public float persistance;
    public float lacuanrity;
    public Vector2 offset;
    public TerrainType[] regions;

    void Start()
    {
        GenerateMap();
        Debug.Log("hallo");
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale,seed, octaves,persistance, lacuanrity, offset);
        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].heightValue)
                    {
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }


        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        mapDisplay.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap,mapWidth,mapHeight)); 
        
    }
	
}
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float heightValue;
    public Color colour;
}

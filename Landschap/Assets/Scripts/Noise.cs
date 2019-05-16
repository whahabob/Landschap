using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

    
	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int seed, int octaves, float persistance, float lacuanirty, Vector2 offset)
    {

        System.Random r = new System.Random(seed);
        Vector2[] octavesOffset = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequencie = 1;
        for(int i = 0; i < octaves; i++)
        {
            float octavesOffsetX = r.Next(-100000, 100000) + offset.x;
            float octavesOffsetY = r.Next(-100000, 100000) - offset.y;
            octavesOffset[i] = new Vector2(octavesOffsetX, octavesOffsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }
        float[,] noiseMap = new float[mapWidth,mapHeight];

        if(scale <= 0)
        {
            scale = 0.0001f;
        }
        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        float centerWidth = mapWidth / 2f;
        float centerHeight = mapHeight / 2f;

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequencie = 1;
                float noiseHeight = 0;


                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - centerWidth + octavesOffset[i].x) / scale * frequencie;
                    float sampleY = (y - centerHeight + octavesOffset[i].y) / scale * frequencie;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    noiseMap[x, y] = perlinValue;

                    amplitude *= persistance;
                    frequencie *= lacuanirty;
                }

                if (noiseHeight > maxHeight)
                    maxHeight = noiseHeight;

                else if (noiseHeight < minHeight)
                    minHeight = noiseHeight;
                
                noiseMap[x, y] = noiseHeight;
            }
        }
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
               // noiseMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[x, y]);
               float normalizedHeight = (noiseMap[x,y] + 1) /(maxPossibleHeight/0.8f);
                noiseMap[x,y] = Mathf.Clamp(normalizedHeight, 0 , int.MaxValue);
            }
        }
        
        return noiseMap;
    }
}

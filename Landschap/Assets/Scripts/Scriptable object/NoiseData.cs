using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData {

    [Range(0, 6)]
    public int octaves;
    public int seed;
    public float noiseScale;
    [Range(0, 1)]
    public float persistance;
    public float lacuanrity;
    public Vector2 offset;

    #if UNITY_EDITOR

    protected override void OnValidate()
    {
        if(lacuanrity < 1)
        {
            lacuanrity = 1;
        }
        if(octaves < 0)
        {
            octaves = 1;
        }
        base.OnValidate();
    }
    #endif
}

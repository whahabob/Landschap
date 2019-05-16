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
}

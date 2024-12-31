using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseMapGenerator
{
    public static float[] GenerateMap(int width, int height, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        return Noise.GenerateNoiseMap(width, height, seed, scale, octaves, persistence, lacunarity, offset);
    }
}

[System.Serializable]
public struct NoiseRegion
{
    public string name;
    [Range(0, 1)] public float heightValue;
    public GameObject prefab;
    public bool canRandomizePosition;
    public Vector2Int randomPositionRange;
}

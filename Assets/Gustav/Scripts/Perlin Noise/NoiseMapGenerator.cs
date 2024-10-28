using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseMapGenerator
{
    public static float[] GenerateMap(int width, int height, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
    {
        if (lacunarity < 1 || octaves < 0)
        {
            return null;
        }

        return Noise.GenerateNoiseMap(width, height, seed, scale, octaves, persistence, lacunarity, offset);
    }
}

[System.Serializable]
public struct PrefabType
{
    public string name;
    public float heightValue;
    public GameObject prefab;
}

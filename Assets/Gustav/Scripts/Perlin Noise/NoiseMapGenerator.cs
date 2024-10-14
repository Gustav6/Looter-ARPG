using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGenerator : MonoBehaviour
{
    public static NoiseMapGenerator Instance { get; private set; }

    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;

    public TerrainType[] regions;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }
    }

    public float[,] GenerateMap(int width, int height, int seed, Vector2 offset)
    {
        return Noise.GenerateNoiseMap(width, height, seed, noiseScale, octaves, persistence, lacunarity, offset);
    }

    public void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float heightValue;
    public GameObject prefab;
}

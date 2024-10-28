using UnityEngine;

public static class Noise
{
    public static float[] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
    {
        float[] map = new float[width * height];

        System.Random rng = new(seed);

        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            octaveOffsets[i] = new Vector2(rng.Next(-100000, 100000) + offset.x, rng.Next(-100000, 100000) + offset.y);
        }

        if (scale <= 0)
        {
            scale = 0.00001f;
        }

        float maxNoiseHeight = float.MinValue, minNoiseHeight = float.MaxValue;

        float amplitude, frequency, noiseHeight, sampleX, sampleY;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                amplitude = 1;
                frequency = 1;
                noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    sampleX = (x - width / 2) / scale * frequency + octaveOffsets[i].x;
                    sampleY = (y - height / 2) / scale * frequency + octaveOffsets[i].y;
                    //float sampleX = ((x - halfWidth) / scale * frequency) + (octaveOffsets[i].x / scale * frequency);
                    //float sampleY = ((y - halfHeight) / scale * frequency) + (octaveOffsets[i].y / scale * frequency);

                    noiseHeight += (Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1) * amplitude;

                    amplitude *= persistence;
                    frequency = lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                map[x + y * width] = noiseHeight;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x + y * width] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, map[x + y * width]);
            }
        }

        return map;
    }
}

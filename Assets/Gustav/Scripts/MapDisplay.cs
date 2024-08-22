using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public Tilemap tileMap;
    public int cellularIterations;

    public Tile floor, wall;
    [Range(0, 1)]
    public float density;

    private int width, height;

    public void DrawNoiseMap(float[,] map)
    {
        tileMap.ClearAllTiles();

        width = map.GetLength(0);
        height = map.GetLength(1);

        for (int i = 0; i < cellularIterations; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < width; k++)
                {
                    int neighborWallCount = 0;

                    for (int y = j - 1; y <= j + 1; y++)
                    {
                        for (int x = k - 1; x <= k + 1; x++)
                        {
                            if (InBounds(x, y))
                            {
                                if (y != j || x != k)
                                {
                                    if (tileMap.GetTile(new Vector3Int(y, x)) == wall)
                                    {
                                        neighborWallCount++;
                                    }
                                }
                            }
                            else
                            {
                                neighborWallCount++;
                            }
                        }
                    }

                    if (neighborWallCount > 4)
                    {
                        tileMap.SetTile(new Vector3Int(j, k), wall);
                    }
                    else
                    {
                        tileMap.SetTile(new Vector3Int(j, k), floor);
                    }

                    tileMap.RefreshTile(new Vector3Int(k, j));
                }
            }
        }

        //texture.SetPixels(colorMap);
        //texture.Apply();

        //textureRenderer.sharedMaterial.mainTexture = texture;
        //textureRenderer.transform.localScale = new Vector3(width, height);
    }
    private bool InBounds(int x, int y)
    {
        return 0 <= y && y < height && 0 <= x && x < width;
    }
}

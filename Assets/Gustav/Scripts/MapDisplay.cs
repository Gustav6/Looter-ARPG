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

    public void DrawNoiseMap(float[,] map)
    {
        tileMap.ClearAllTiles();

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        //Texture2D texture = new(width, height);

        //Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map[x, y] <= density)
                {
                    tileMap.SetTile(new Vector3Int(x, y), floor);
                }
                else
                {
                    tileMap.SetTile(new Vector3Int(x, y), wall);
                }
            }
        }


        for (int i = 0; i < cellularIterations; i++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                }
            }
        }

        //texture.SetPixels(colorMap);
        //texture.Apply();

        //textureRenderer.sharedMaterial.mainTexture = texture;
        //textureRenderer.transform.localScale = new Vector3(width, height);
    }
}

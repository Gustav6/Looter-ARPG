using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [SerializeField]
    private Tilemap interactableMap;

    [SerializeField]
    private List<TileData> tileData;

    private Dictionary<TileBase, TileData> dataFromTiles;

    private void Awake()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (TileData tileData in tileData)
        {
            foreach (TileBase tile in tileData.tiles)
            {
                dataFromTiles.TryAdd(tile, tileData);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector3Int tile = interactableMap.layoutGrid.WorldToCell(collision.gameObject.transform.position);

        Debug.Log(tile);
    }
}

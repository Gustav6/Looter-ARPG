using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    #region States
    private MapBaseState currentState;
    public GeneratingMapState generationState = new();
    public LoadedMapState loadedState = new();
    #endregion

    #region Tile & TileMap
    [BoxGroup("TileMap Variables")]
    [Required("Ground tile map is needed")] public Tilemap groundTileMap;
    [BoxGroup("TileMap Variables")]
    [Required("Wall tile map is needed")] public Tilemap wallTileMap;
    [BoxGroup("TileMap Variables")]
    public TilePair[] tiles;
    #endregion

    public MapSettings Settings { get; private set; }

    public Room startingRoom;

    public List<Edge> minimumSpanningTree;
    public Room[] rooms;

    public readonly Dictionary<TileTexture, TileBase> tilePairs = new();

    public GameObject activeGameObjectsParent;
    public List<GameObject> gameObjectsList = new();

    public Dictionary<Vector2Int, List<GameObject>> regions;
    public int RegionWidth { get; private set; }
    public int RegionHeight { get; private set; }

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

        RegionHeight = 16;
        RegionWidth = 16;

        Settings = GetComponent<MapSettings>();

        foreach (TilePair pair in tiles)
        {
            tilePairs.Add(pair.type, pair.tile);
        }

        SwitchState(generationState);
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && currentState == loadedState)
        {
            SwitchState(generationState);
        }

        currentState?.UpdateState(this);
    }

    public void SwitchState(MapBaseState state)
    {
        currentState?.ExitState(this);

        currentState = state;

        currentState.EnterState(this);
    }

    public void SpawnPrefab(GameObject prefab, Vector3Int tileSpawnPosition, bool activeStatus = true)
    {
        if (startingRoom.groundTiles.Contains(tileSpawnPosition) && Settings.doNotAllowInStartingRoom.Contains(prefab))
        {
            return;
        }

        GameObject spawnedPrefab = Instantiate(prefab, tileSpawnPosition + new Vector3(0.5f, 0.5f), Quaternion.identity, activeGameObjectsParent.transform);
        SetGameObjectsRegion(spawnedPrefab);
        spawnedPrefab.SetActive(activeStatus);
        gameObjectsList.Add(spawnedPrefab);
    }

    public void SetGameObjectsRegion(GameObject gameObject)
    {
        if (!gameObjectsList.Contains(gameObject))
        {
            gameObjectsList.Add(gameObject);
        }

        Vector2Int region = new((int)(gameObject.transform.position.x / RegionWidth), (int)(gameObject.transform.position.y / RegionHeight));

        if (regions.ContainsKey(region))
        {
            regions[region].Add(gameObject);
        }
        else
        {
            regions.Add(region, new List<GameObject> { gameObject });
        }
    }

    public void RemoveGameObjectFromMap(GameObject g)
    {
        if (regions.ContainsKey(new((int)(g.transform.position.x / RegionWidth), (int)(g.transform.position.y / RegionHeight))))
        {
            regions[new((int)(g.transform.position.x / RegionWidth), (int)(g.transform.position.y / RegionHeight))].Remove(g);
        }
        gameObjectsList.Remove(g);
    }

    public static T OneToTwoDimensional<T>(Vector2Int position, T[] grid, int width)
    {
        int index = position.x + position.y * width;

        return grid[index];
    }

    public static T TwoToOneDimensional<T>(int index, T[,] grid, int width)
    {
        int x = index % width;
        int y = index / width;

        return grid[x, y];
    }
}

public enum SpawnFunction
{
    Circle,
    Strip
}

public enum TileTexture
{
    ground,   
    wall,
}

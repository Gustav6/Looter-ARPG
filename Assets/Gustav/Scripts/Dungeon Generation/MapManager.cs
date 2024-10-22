using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
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

    public List<Room> activeRooms = new();

    public Room startingRoom;

    public Map currentMap;

    public List<Edge> connectedRooms;
    public List<Room> rooms;

    [field: SerializeField] public GameObject PlayerReference { get; private set; }

    public readonly Dictionary<TileTexture, TileBase> tilePairs = new();

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

        foreach (TilePair pair in tiles)
        {
            tilePairs.Add(pair.type, pair.tile);
        }

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            PlayerReference = GameObject.FindGameObjectWithTag("Player");
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

    public void LoadMap(Map mapToLoad)
    {
        MapSettings.Instance.seed = mapToLoad.seed;
        MapSettings.Instance.spawnFunction = mapToLoad.spawnFunction;
        MapSettings.Instance.roomMaxSize = mapToLoad.roomMaxSize;
        MapSettings.Instance.RoomMinSize = mapToLoad.roomMinSize;
        MapSettings.Instance.totalRoomsCount = mapToLoad.amountOfRooms;
        MapSettings.Instance.AmountOfMainRooms = mapToLoad.amountOfMainRooms;
        MapSettings.Instance.amountOfLoops = mapToLoad.amountOfLoops;
        MapSettings.Instance.hallwayWidth = mapToLoad.hallwayWidth;
        MapSettings.Instance.stripSize = mapToLoad.stripSpawnSize;
        MapSettings.Instance.generationRadius = mapToLoad.spawnRadius;

        SwitchState(generationState);
    }

    public static T TwoToOneDimensional<T>(Vector2Int position, T[] grid, int width)
    {
        int index = position.x + position.y * width;

        return grid[index];
    }

    public static T OneToTwoDimensional<T>(int index, T[,] grid, int width)
    {
        int x = index % width;
        int y = index / width;

        return grid[x, y];
    }

    [Serializable]
    public struct Map
    {
        public int seed;
        public SpawnFunction spawnFunction;
        public int spawnRadius;
        public Vector2Int stripSpawnSize;
        public int amountOfRooms, amountOfMainRooms;
        public Vector2Int roomMinSize, roomMaxSize;
        public float amountOfLoops;
        public int hallwayWidth;
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

using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapSettings : MonoBehaviour
{
    [Foldout("Generation Variables")]
    public int seed;

    [Foldout("Generation Variables")]
    public SpawnFunction spawnFunction = SpawnFunction.Circle;

    [Foldout("Generation Variables")]
    public int radiusForGen = 50;

    [Foldout("Generation Variables")]
    public Vector2Int stripSizeForGen = new(100, 50);

    [Foldout("Room Variables")]
    public int totalRoomsCount = 10;

    [Foldout("Room Variables")]
    [SerializeField] private int amountOfMainRooms = 5;
    public int AmountOfMainRooms
    {
        get { return amountOfMainRooms; }
        set
        {
            if (value > totalRoomsCount)
            {
                amountOfMainRooms = totalRoomsCount;
            }
            else
            {
                amountOfMainRooms = value;
            }
        }
    }

    [Foldout("Room Variables")]
    public Vector2Int roomMaxSize = new(10, 10);

    [Foldout("Room Variables")]
    [SerializeField] private Vector2Int roomMinSize = new(5, 5);
    public Vector2Int RoomMinSize
    {
        get { return roomMinSize; }
        set
        {
            if (value.x <= roomMaxSize.x && value.y <= roomMaxSize.y)
            {
                roomMinSize = value;
            }
            else
            {
                if (value.x > roomMaxSize.x)
                {
                    roomMinSize.x = roomMaxSize.x;
                }
                if (value.y > roomMaxSize.y)
                {
                    roomMinSize.y = roomMaxSize.y;
                }
            }
        }
    }

    [Foldout("Room Variables")]
    public GameObject[] doNotAllowInStartingRoom;

    [Foldout("Hallway Variables")]
    [Range(0, 100)] public float amountOfHallwayLoops = 15;

    [Foldout("Hallway Variables")]
    public bool randomizedHallwayWidth = false;

    [Foldout("Hallway Variables")]
    [DisableIf("randomizedHallwayWidth")] public int hallwayWidth = 10;

    [Foldout("Hallway Variables")]
    [EnableIf("randomizedHallwayWidth")] public int hallwayMinWidth = 10;

    [Foldout("Hallway Variables")]
    [EnableIf("randomizedHallwayWidth")] public int hallwayMaxWidth = 15;

    [Foldout("Enemy Variables")]
    public GameObject[] enemyPrefabs;

    [Foldout("Enemy Variables")]
    public int amountOfEnemies;

    [Foldout("Perlin Noise Map")]
    public NoiseMap[] noiseMaps;

    [Foldout("Spawn Prefab")]
    public PrefabSpawn[] prefabsToSpawn;
}

[Serializable]
public struct NoiseMap
{
    public TileMapType tileMapEffected;

    public int amountOfNoiseLoops;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)] public float persistence;
    public float lacunarity;
    public NoiseRegion[] prefabs;
}

[Serializable]
public struct TilePair
{
    public TileBase tile;
    public TileTexture type;
}

[Serializable]
public struct PrefabSpawn
{
    public int amount;
    public GameObject prefab;
    public TileMapType tileMapEffected;
    public bool canRandomizePosition;
    public Vector2Int randomPositionRange;
}
using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapSettings : MonoBehaviour
{
    #region Generation
    [Foldout("Generation Variables")]
    public int seed;

    [Foldout("Generation Variables")]
    public SpawnFunction spawnFunction = SpawnFunction.Circle;

    [Foldout("Generation Variables")]
    public int generationRadius = 50;

    [Foldout("Generation Variables")]
    public Vector2Int stripSize = new(100, 50);
    #endregion

    #region Room

    [Foldout("Room Variables")]
    public int totalRoomsCount = 10;

    #region Main room variable
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
    #endregion

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
    public bool roundCorners = true;
    [Foldout("Room Variables")]
    public GameObject[] doNotAllowInStartingRoom;

    #endregion

    #region Hallway

    [Foldout("Hallway Variables")]
    [Range(0, 100)] public float amountOfHallwayLoops = 15;
    [Foldout("Hallway Variables")]
    public bool randomizedHallwaySize = false;
    [Foldout("Hallway Variables")]
    [DisableIf("randomizedHallwaySize")] public int hallwayWidth = 10;
    [Foldout("Hallway Variables")]
    [EnableIf("randomizedHallwaySize")] public int hallwayMinWidth = 10;
    [Foldout("Hallway Variables")]
    [EnableIf("randomizedHallwaySize")] public int hallwayMaxWidth = 15;

    #endregion

    #region Enemies

    [Foldout("Enemy Variables")]
    public GameObject[] enemyPrefabs;

    [Foldout("Enemy Variables")]
    public int amountOfEnemies;

    #endregion

    #region Perlin Noise
    [Foldout("Perlin Noise")]
    public int amountOfNoiseLoops;
    [Foldout("Perlin Noise")]
    public float noiseScale;
    [Foldout("Perlin Noise")]
    public int octaves;
    [Foldout("Perlin Noise")]
    [Range(0, 1)] public float persistence;
    [Foldout("Perlin Noise")]
    public float lacunarity;
    [Foldout("Perlin Noise")]
    public NoiseRegion[] prefabs;
    #endregion
}

[Serializable]
public struct TilePair
{
    public TileBase tile;
    public TileTexture type;
}
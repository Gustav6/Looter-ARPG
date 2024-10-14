using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapSettings : MonoBehaviour
{
    public static MapSettings Instance { get; private set; }

    private void Awake()
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

    #region Generation
    [BoxGroup("Generation Variables")]
    public int seed;

    [BoxGroup("Generation Variables")]
    public SpawnFunction spawnFunction = SpawnFunction.Circle;

    [BoxGroup("Generation Variables")]
    public int generationRadius = 50;

    [BoxGroup("Generation Variables")]
    public Vector2Int stripSize = new(100, 50);
    #endregion

    #region Room
    [BoxGroup("Room Variables")]
    public int totalRoomsCount = 10;

    #region Main room variable
    [BoxGroup("Room Variables")]
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

    [BoxGroup("Room Variables")]
    public Vector2Int roomMaxSize = new(10, 10);

    [BoxGroup("Room Variables")]
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

    [BoxGroup("Room Variables")]
    public bool roundCorners = true;
    #endregion

    #region Hallway
    [BoxGroup("Hallway Variables")]
    [Range(0, 100)] public float amountOfLoops = 15;
    [BoxGroup("Hallway Variables")]
    public bool randomizedHallwaySize = false;
    [BoxGroup("Hallway Variables")]
    [DisableIf("randomizedHallwaySize")] public int hallwayWidth = 10;
    [BoxGroup("Hallway Variables")]
    [EnableIf("randomizedHallwaySize")] public int hallwayMinWidth = 10;
    [BoxGroup("Hallway Variables")]
    [EnableIf("randomizedHallwaySize")] public int hallwayMaxWidth = 15;
    #endregion

    #region Traps
    [BoxGroup("Trap Variables")]
    [Range(0, 100)] public float amountOfTraps = 10;
    #endregion

    #region Debug
    [Foldout("Debug")]
    public bool debugTriangulation = false;
    [Foldout("Debug")]
    public bool debugSpanningTree = false;
    #endregion
}

[Serializable]
public struct TilePair
{
    public TileBase tile;
    public TileTexture type;
}
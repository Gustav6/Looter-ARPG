using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerationManager : MonoBehaviour
{
    #region States

    private MapBaseState currentState;
    public GeneratingMapState generationState = new();
    public LoadMapState loadingState = new();

    #endregion

    public static MapGenerationManager instance;

    [Header("Spawn Variables")]
    public bool generateInCircle = false;
    public int generationRadius = 50;

    public bool generateInStrip = true;
    public Vector2 stripSize = new Vector2Int(100, 20);

    #region Squared room variables
    [Header("Square room")]
    public bool generateSquareRooms = true;
    public bool roundCornors = true;

    public Vector2Int squaredRoomMaxSize = new(10, 10);

    [SerializeField]
    private Vector2Int squaredRoomMinSize = new(5, 5);
    public Vector2Int SquaredRoomMinSize
    {
        get { return squaredRoomMinSize; }
        set
        {
            if (value.x <= squaredRoomMaxSize.x && value.y <= squaredRoomMaxSize.y)
            {
                squaredRoomMinSize = value;
            }
            else
            {
                if (value.x > squaredRoomMaxSize.x)
                {
                    squaredRoomMinSize.x = squaredRoomMaxSize.x;
                }
                if (value.y > squaredRoomMaxSize.y)
                {
                    squaredRoomMinSize.y = squaredRoomMaxSize.y;
                }
            }
        }
    }
    #endregion

    #region Circle room variables
    [Header("Circle room")]
    public bool generateCircleRooms = false;
    public int circleRoomMaxSize = 15;
    [SerializeField]
    private int circleRoomMinSize = 5;
    public int CircleRoomMinSize
    {
        get { return circleRoomMinSize; }
        set
        {
            if (value <= circleRoomMinSize)
            {
                circleRoomMinSize = value;
            }
            else
            {
                circleRoomMinSize = circleRoomMaxSize;
            }
        }
    }
    #endregion

    [Header("Hallway")]
    [Range(0, 100)]
    public float amountOfLoops = 15;
    public int hallwayWidth = 10;

    [Header("Room count variables")]
    public int totalRoomsCount = 10;

    #region Main room variable
    [SerializeField]
    private int amountOfMainRooms = 5;
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

    [Header("Needed to run")]

    public Tilemap groundTileMap;
    public Tilemap wallTileMap;
    public TileBase ground, wall;

    public readonly Dictionary<TileTexture, TileBase> tilePairs = new();

    private void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;

        tilePairs.Add(TileTexture.ground, ground);
        tilePairs.Add(TileTexture.wall, wall);

        SwitchState(generationState);
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && currentState == loadingState)
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
}

public enum TileTexture
{
    ground,
    wall,
}

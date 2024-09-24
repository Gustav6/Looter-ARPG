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

    public int generationRadius;

    #region Room size variables
    public Vector2Int roomMaxSize;
    private Vector2Int roomMinSize;
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
    #endregion

    #region Main room variable
    private int amountOfMainRooms;
    public int AmountOfMainRooms
    {
        get { return amountOfMainRooms; }
        set
        {
            if (value > totalRoomsAmount)
            {
                amountOfMainRooms = totalRoomsAmount;
            }
            else
            {
                amountOfMainRooms = value;
            }
        }
    }
    #endregion

    public TileBase tileTexture;
    public Tilemap tileMap;
    public int totalRoomsAmount;

    public bool showEveryRoom = false;

    public TileBase debugTexture;

    private void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;

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
}

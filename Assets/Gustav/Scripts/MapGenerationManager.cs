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
    public Vector2Int roomMaxSize;

    private int mainRoomDepth;
    public int MainRoomDepth
    {
        get { return mainRoomDepth; }
        set
        {
            if (value >= amountOfRooms / 2)
            {
                mainRoomDepth = amountOfRooms / 2 - 1;
            }
            else
            {
                mainRoomDepth = value;
            }
        }
    }

    public TileBase tileTexture;
    public Tilemap tileMap;
    public int amountOfRooms;

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
        currentState?.UpdateState(this);
    }

    public void SwitchState(MapBaseState state)
    {
        currentState?.ExitState(this);

        currentState = state;

        state.EnterState(this);
    }
}

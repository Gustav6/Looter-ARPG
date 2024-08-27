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

    public TileBase tileTexture;
    public Tilemap tileMap;
    public int amountOfRooms;

    // Inom tilemap på uni
    public const int tileSize = 100 / 64;

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
        state.ExitState(this);

        currentState = state;

        state.EnterState(this);
    }
}

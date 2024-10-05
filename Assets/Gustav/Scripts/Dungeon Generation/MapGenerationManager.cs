using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerationManager : MonoBehaviour
{
    #region States

    private MapBaseState currentState;
    public GeneratingMapState generationState = new();
    public LoadedMapState loadedState = new();

    #endregion

    public Room startingRoom;

    public Map map;

    public readonly Dictionary<TileTexture, TileBase> tilePairs = new();

    private void Start()
    {
        tilePairs.TryAdd(TileTexture.ruleTile, MapSettings.Instance.ruleTile);

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
}

public enum SpawnFunction
{
    Circle,
    Strip
}

public enum TileTexture
{
    ruleTile,   
    wall,
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

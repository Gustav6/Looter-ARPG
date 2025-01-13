using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    [field: SerializeField] public Tilemap GroundMap { get; private set; }
    [field: SerializeField] public Tilemap WallMap { get; private set; }
    [field: SerializeField] public Tilemap WallMapIcons { get; private set; }
    [field: SerializeField] public GameObject ActiveGameObjectsParent { get; private set; }

    public int amountOfCoreroutinesStarted, amountOfCoreroutinesFinished;

    public Dictionary<Vector2Int, List<GameObject>> MapRegions { get; set; }

    public Room startRoom, endRoom;

    public bool readyToLoad = false;

    [field: SerializeField] public RequiredSettings RequiredSettingsForMap { get; private set; }

    public void SetRequiredSettings()
    {
        RequiredSettingsForMap = new()
        {
            seed = MapManager.Instance.Settings.seed,

            // Rooms
            totalRoomsCount = MapManager.Instance.Settings.totalRoomsCount,
            amountOfMainRooms = MapManager.Instance.Settings.AmountOfMainRooms,
            roomMaxSize = MapManager.Instance.Settings.roomMaxSize,
            roomMinSize = MapManager.Instance.Settings.RoomMinSize,

            // Spawn
            spawnFunction = MapManager.Instance.Settings.spawnFunction,
            radiusForGen = MapManager.Instance.Settings.radiusForGen,
            stripSizeForGen = MapManager.Instance.Settings.stripSizeForGen,

            // Hallways
            amountOfHallwayLoops = MapManager.Instance.Settings.amountOfHallwayLoops,
            randomizedHallwayWidth = MapManager.Instance.Settings.randomizedHallwayWidth,
            hallwayWidth = MapManager.Instance.Settings.hallwayWidth,
            hallwayMinWidth = MapManager.Instance.Settings.hallwayMinWidth,
            hallwayMaxWidth = MapManager.Instance.Settings.hallwayMaxWidth,

            // Enemies
            amountOfEnemies = MapManager.Instance.Settings.amountOfEnemies
        };
    }
}

[Serializable]
public struct RequiredSettings
{
    // Variable for random number gen
    public int seed;

    // Room variables
    public int totalRoomsCount, amountOfMainRooms;
    public Vector2Int roomMaxSize, roomMinSize;

    // Spawn variables
    public SpawnFunction spawnFunction;
    public int radiusForGen;
    public Vector2Int stripSizeForGen;

    // Hallway variables
    public float amountOfHallwayLoops;
    public bool randomizedHallwayWidth;
    public int hallwayWidth, hallwayMinWidth, hallwayMaxWidth;

    // Enemies
    public int amountOfEnemies;
}

using System;
using UnityEngine;

public class GameData
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

    // Player
    public int playerCurrentHealth, playerMaxHealth;

    // Will define default values when game data has not been created
    public GameData()
    {
        seed = MapManager.Instance.Settings.seed;

        // Rooms
        totalRoomsCount = MapManager.Instance.Settings.totalRoomsCount;
        amountOfMainRooms = MapManager.Instance.Settings.AmountOfMainRooms;
        roomMaxSize = MapManager.Instance.Settings.roomMaxSize;
        roomMinSize = MapManager.Instance.Settings.RoomMinSize;

        // Spawn
        spawnFunction = MapManager.Instance.Settings.spawnFunction;
        radiusForGen = MapManager.Instance.Settings.radiusForGen;
        stripSizeForGen = MapManager.Instance.Settings.stripSizeForGen;

        // Hallways
        amountOfHallwayLoops = MapManager.Instance.Settings.amountOfHallwayLoops;
        randomizedHallwayWidth = MapManager.Instance.Settings.randomizedHallwayWidth;
        hallwayWidth = MapManager.Instance.Settings.hallwayWidth;
        hallwayMinWidth = MapManager.Instance.Settings.hallwayMinWidth;
        hallwayMaxWidth = MapManager.Instance.Settings.hallwayMaxWidth;

        // Enemies
        amountOfEnemies = MapManager.Instance.Settings.amountOfEnemies;

        playerMaxHealth = Player.Instance.MaxHealth;
        playerCurrentHealth = playerMaxHealth;
    }
}

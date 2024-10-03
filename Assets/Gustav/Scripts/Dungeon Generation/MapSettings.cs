using NaughtyAttributes;
using NaughtyAttributes.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapSettings : MonoBehaviour
{
    #region Generation
    [BoxGroup("Generation Variables")]
    public bool generateInCircle = true;

    [BoxGroup("Generation Variables")]
    [ShowIf("generateInCircle")] public int generationRadius = 50;

    [BoxGroup("Generation Variables")]
    public bool generateInStrip = false;

    [BoxGroup("Generation Variables")]
    [ShowIf("generateInStrip")] public Vector2 stripSize = new Vector2Int(100, 50);
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
    public Vector2Int squaredRoomMaxSize = new(10, 10);

    [BoxGroup("Room Variables")]
    [SerializeField] private Vector2Int squaredRoomMinSize = new(5, 5);
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

    [BoxGroup("Room Variables")]
    public bool roundCorners = true;
    #endregion

    #region Hallway
    [BoxGroup("Hallway Variables")]
    [Range(0, 100)] public float amountOfLoops = 15;
    [BoxGroup("Hallway Variables")]
    public int hallwayWidth = 10;
    #endregion

    #region Tile & TileMap
    [BoxGroup("TileMap Variables")]
    [Required] public Tilemap groundTileMap;
    [BoxGroup("TileMap Variables")]
    [Required] public TileBase ruleTile;
    #endregion

    #region Debug
    [BoxGroup("Debug Variables")]
    public bool debugTriangulation = false;
    [BoxGroup("Debug Variables")]
    public bool debugSpanningTree = false;
    #endregion
}

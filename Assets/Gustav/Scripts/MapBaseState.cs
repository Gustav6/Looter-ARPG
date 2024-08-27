using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

#region Base State
public abstract class MapBaseState
{
    public abstract void EnterState(MapGenerationManager manager);
    public abstract void UpdateState(MapGenerationManager manager);
    public abstract void ExitState(MapGenerationManager manager);
}
#endregion

#region Generation State
public class GeneratingMapState : MapBaseState
{
    public List<Room> rooms = new();

    public override void EnterState(MapGenerationManager manager)
    {
        for (int i = 0; i < MapGenerationManager.instance.amountOfRooms; i++)
        {
            rooms.Add(GenerateRoom());

            GameObject g = new();
            g.transform.position = rooms[i].WorldPosition;
        }

        PlaceRooms();
    }

    public override void UpdateState(MapGenerationManager manager)
    {
        SeparateRooms();
    }

    public override void ExitState(MapGenerationManager manager)
    {
    }

    #region Generate Room methods
    private Room GenerateRoom()
    {
        int roomWidth = UnityEngine.Random.Range(5, MapGenerationManager.instance.roomMaxSize.x + 1);
        int roomHeight = UnityEngine.Random.Range(5, MapGenerationManager.instance.roomMaxSize.y + 1);
        
        Vector2Int offset = new(roomWidth / 2, roomHeight / 2);
        Vector2 position = RandomPosition(MapGenerationManager.instance.generationRadius) - offset;

        return new Room(roomWidth, roomHeight, new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)));
    }

    public Vector2 RandomPosition(float radius)
    {
        float r = radius * Mathf.Sqrt(UnityEngine.Random.Range(0.0001f, 1));
        float theta = UnityEngine.Random.Range(0.0001f, 1) * 2 * Mathf.PI;

        return new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
        //return new Vector2(RoundM(r * Mathf.Cos(theta), MapGenerationManager.tileSize), RoundM(r * Mathf.Sin(theta), MapGenerationManager.tileSize));
    }
    #endregion

    //public float RoundM(float n, float m)
    //{
    //    return math.floor((n + m - 1) / m) * m;
    //}

    private void PlaceRooms()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int x = 0; x < rooms[i].tiles.GetLength(0); x++)
            {
                for (int y = 0; y < rooms[i].tiles.GetLength(1); y++)
                {
                    MapGenerationManager.instance.tileMap.SetTile((Vector3Int)rooms[i].tiles[x, y].gridPosition, MapGenerationManager.instance.tileTexture);
                }
            }
        }
    }

    private void SeparateRooms()
    {

    }

    private List<Room> GetMainRooms()
    {
        // Find the rooms with biggest area and return those as main rooms

        return new List<Room>();
    }

    private void GetShortestSpanningTree()
    {

    }

    private void PlaceWalls()
    {

    }
}
#endregion

#region Load State
public class LoadMapState : MapBaseState
{
    public override void EnterState(MapGenerationManager manager)
    {

    }

    public override void UpdateState(MapGenerationManager manager)
    {

    }

    public override void ExitState(MapGenerationManager manager)
    {

    }
}
#endregion

public class Room
{
    public Vector2 WorldPosition
    {
        get
        {
            Vector2 worldPosition = MapGenerationManager.instance.tileMap.CellToWorld((Vector3Int)tiles[width - 1, height - 1].gridPosition) + Vector3.one;

            worldPosition.x -= width / 2f;
            worldPosition.y -= height / 2f;

            return worldPosition;
        }
    }

    public RoomTile[,] tiles;
    public readonly int width, height;

    public Room(int width, int height, Vector2 position)
    {
        this.width = width;
        this.height = height;
        tiles = new RoomTile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int xPosition = x + (int)position.x;
                int yPosition = y + (int)position.y;

                tiles[x, y] = new RoomTile(new Vector2Int(xPosition, yPosition));
            }
        }
    }

    public void MoveRoom(Vector2Int direction)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y].gridPosition += direction;
            }
        }
    }
}

public struct RoomTile
{
    public Vector2Int gridPosition;

    public RoomTile(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }
}
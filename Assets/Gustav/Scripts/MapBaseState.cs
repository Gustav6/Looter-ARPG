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

            Object.Instantiate(new GameObject(), rooms[i].WorldPosition, Quaternion.identity);
        }

        PlaceRooms();
    }

    public override void UpdateState(MapGenerationManager manager)
    {

    }

    public override void ExitState(MapGenerationManager manager)
    {

    }

    private Room GenerateRoom()
    {
        int roomWidth = UnityEngine.Random.Range(5, MapGenerationManager.instance.roomMaxSize.x + 1);
        int roomHeight = UnityEngine.Random.Range(5, MapGenerationManager.instance.roomMaxSize.y + 1);
        
        Vector2Int offset = new(roomWidth / 2, roomHeight / 2);
        Vector2 position = RandomPosition(MapGenerationManager.instance.generationRadius) - offset;

        return new Room(roomWidth, roomHeight, position);
    }

    public Vector2 RandomPosition(float radius)
    {
        float r = radius * Mathf.Sqrt(UnityEngine.Random.Range(0.0001f, 1));
        float theta = UnityEngine.Random.Range(0.0001f, 1) * 2 * Mathf.PI;

        return new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
        //return new Vector2(RoundM(r * Mathf.Cos(theta), MapGenerationManager.tileSize), RoundM(r * Mathf.Sin(theta), MapGenerationManager.tileSize));
    }

    public float RoundM(float n, float m)
    {
        return math.floor((n + m - 1) / m) * m;
    }

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

    private void GetMainRooms()
    {
        // Get rooms with biggest area and return those as main rooms
    }

    private void GetDelaunayTriangulation()
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
    public Vector2 WorldPosition { get { return position; } }
    private Vector2 position;

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

        Vector2 worldPosition = MapGenerationManager.instance.tileMap.CellToWorld((Vector3Int)tiles[0, 0].gridPosition);
        //Vector2 worldPosition = MapGenerationManager.instance.tileMap.CellToWorld((Vector3Int)tiles[width - 1, height - 1].gridPosition);
        //worldPosition.x -= Mathf.Abs(MapGenerationManager.instance.tileMap.CellToWorld((Vector3Int)tiles[0, 0].gridPosition).x);
        //worldPosition.y -= Mathf.Abs(MapGenerationManager.instance.tileMap.CellToWorld((Vector3Int)tiles[0, 0].gridPosition).y);

        this.position = worldPosition;
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
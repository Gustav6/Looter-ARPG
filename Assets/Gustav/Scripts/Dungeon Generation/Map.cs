using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    [field: SerializeField] public Tilemap GroundMap { get; private set; }
    [field: SerializeField] public Tilemap WallMap { get; private set; }
    [field: SerializeField] public Tilemap WallMapIcons { get; private set; }
    [field: SerializeField] public GameObject ActiveGameObjectsParent { get; private set; }

    public Dictionary<Vector2Int, List<GameObject>> MapRegions { get; set; }

    public Room startRoom, endRoom;

    public bool generationComplete = false;
}

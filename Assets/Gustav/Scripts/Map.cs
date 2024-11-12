using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    [field: SerializeField] public Tilemap GroundMap { get; private set; }
    [field: SerializeField] public Tilemap WalldMap { get; private set; }
    [field: SerializeField] public GameObject ActiveGameObjects { get; private set; }

    public Dictionary<Vector2Int, List<GameObject>> MapRegions { get; set; }

}

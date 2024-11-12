using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    #region Tiles
    [BoxGroup("Tile Variables")]
    public TilePair[] tiles;
    #endregion

    public readonly Dictionary<TileTexture, TileBase> tilePairs = new();
    public CancellationTokenSource TokenSource { get; private set; }
    public MapSettings Settings { get; private set; }

    [field: SerializeField] public int RegionWidth { get; private set; }
    [field: SerializeField] public int RegionHeight { get; private set; }

    public Dictionary<Vector2Int, List<GameObject>> currentMapRegions, nextMapRegions;
    private readonly HashSet<Vector2Int> previousRegions = new(), currentRegions = new();

    private Camera cameraReference;
    private Vector2Int cBottomLeft, cTopRight, region;

    public GameObject activeGameObjectsParent;

    [field: SerializeField] public GameObject MapPrefab { get; private set; }

    public GameObject currentMap, nextMap;
    public Room startingRoom;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }

        TokenSource = new();

        RegionHeight = 16;
        RegionWidth = 16;

        Settings = GetComponent<MapSettings>();

        cameraReference = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Player.Instance.OnRegionSwitch += Instance_OnRegionSwitch;

        foreach (TilePair pair in tiles)
        {
            tilePairs.Add(pair.type, pair.tile);
        }

        MapGeneration.GenerateMapAsync(this, MapPrefab);

        MapGeneration.OnGenerationCompleted += MapGeneration_OnGenerationCompleted;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ChangeMap();
        }
    }

    private void MapGeneration_OnGenerationCompleted(object sender, EventArgs e)
    {
        if (currentMap != null && !currentMap.activeInHierarchy)
        {
            LoadCurrentMap();
        }

        Settings.seed++;

        if (nextMap == null)
        {
            MapGeneration.GenerateMapAsync(this, MapPrefab);
        }
    }

    #region Load and change map methods
    public void LoadCurrentMap()
    {
        currentMap.SetActive(true);

        Player.Instance.transform.position = startingRoom.WorldPosition;
        cameraReference.transform.position = Player.Instance.transform.position;
        UpdateActiveRegions();
    }

    public void ChangeMap()
    {
        if (currentMap != null && nextMap != null)
        {
            GameObject destroyAfterSwitch = currentMap;

            currentMap = nextMap;
            currentMapRegions = nextMapRegions;

            currentMap.name = "Active Map";

            nextMapRegions = null;
            nextMap = null;

            Destroy(destroyAfterSwitch);
            LoadCurrentMap();

            //MapGeneration.GenerateMapAsync(this, MapPrefab);
        }
    }
    #endregion

    #region Update regions status
    private void Instance_OnRegionSwitch(object sender, EventArgs e)
    {
        if (currentMap != null && currentMap.activeInHierarchy)
        {
            UpdateActiveRegions();
        }
    }

    private void UpdateActiveRegions()
    {
        currentRegions.Clear();

        cBottomLeft = Vector2Int.FloorToInt(0.0625f * cameraReference.ScreenToWorldPoint(new Vector3(0, 0, cameraReference.nearClipPlane)));
        cTopRight = Vector2Int.FloorToInt(0.0625f * cameraReference.ScreenToWorldPoint(new Vector3(cameraReference.pixelWidth, cameraReference.pixelHeight, cameraReference.nearClipPlane)));

        for (int x = cBottomLeft.x - 1; x < cTopRight.x + 2; x++)
        {
            for (int y = cBottomLeft.y - 1; y < cTopRight.y + 2; y++)
            {
                region = new(x, y);

                if (!currentMapRegions.TryGetValue(region, out var enableList))
                {
                    continue;
                }
                else if (previousRegions.Contains(region))
                {
                    previousRegions.Remove(region);
                    currentRegions.Add(region);
                    continue;
                }

                for (int i = 0; i < enableList.Count; i++)
                {
                    enableList[i].SetActive(true);
                }

                currentRegions.Add(region);
            }
        }

        foreach (Vector2Int previousRegion in previousRegions)
        {
            if (currentMapRegions.TryGetValue(previousRegion, out var disableList))
            {
                for (int i = 0; i < disableList.Count; i++)
                {
                    disableList[i].SetActive(false);
                }
            }
        }

        previousRegions.Clear();
        previousRegions.UnionWith(currentRegions);
    }
    #endregion

    private void OnDisable()
    {
        TokenSource.Cancel();
    }

    public GameObject SpawnPrefab(GameObject map, GameObject prefab, Vector3Int tileSpawnPosition, Transform parent, bool activeStatus = true)
    {
        if (startingRoom.groundTiles.Contains(tileSpawnPosition) && Settings.doNotAllowInStartingRoom.Contains(prefab))
        {
            return null;
        }

        GameObject spawnedPrefab = Instantiate(prefab, tileSpawnPosition + new Vector3(0.5f, 0.5f), Quaternion.identity, parent);
        SetGameObjectsRegion(spawnedPrefab, map);
        spawnedPrefab.SetActive(activeStatus);

        return spawnedPrefab;
    }

    public void SetGameObjectsRegion(GameObject gameObject, GameObject map)
    {
        Vector2Int region = new((int)(gameObject.transform.position.x / RegionWidth), (int)(gameObject.transform.position.y / RegionHeight));

        if (map == currentMap)
        {
            if (currentMapRegions.ContainsKey(region))
            {
                currentMapRegions[region].Add(gameObject);
            }
            else
            {
                currentMapRegions.Add(region, new List<GameObject> { gameObject });
            }
        }
        else if (map == nextMap)
        {
            if (nextMapRegions.ContainsKey(region))
            {
                nextMapRegions[region].Add(gameObject);
            }
            else
            {
                nextMapRegions.Add(region, new List<GameObject> { gameObject });
            }
        }
    }

    public void RemoveGameObjectFromMap(GameObject g, GameObject map)
    {
        if (map == currentMap)
        {
            if (currentMapRegions.ContainsKey(new((int)(g.transform.position.x / RegionWidth), (int)(g.transform.position.y / RegionHeight))))
            {
                currentMapRegions[new((int)(g.transform.position.x / RegionWidth), (int)(g.transform.position.y / RegionHeight))].Remove(g);
            }
        }
        else if (map == nextMap)
        {
            if (nextMapRegions.ContainsKey(new((int)(g.transform.position.x / RegionWidth), (int)(g.transform.position.y / RegionHeight))))
            {
                nextMapRegions[new((int)(g.transform.position.x / RegionWidth), (int)(g.transform.position.y / RegionHeight))].Remove(g);
            }
        }
    }
}

public enum TileMapType
{
    ground,
    wall
}

public enum SpawnFunction
{
    Circle,
    Strip
}

public enum TileTexture
{
    ground,   
    wall,
}

using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour, IDataPersistence
{
    public static MapManager Instance { get; private set; }

    [BoxGroup("Array of tiles")]
    public TilePair[] tiles;

    public readonly Dictionary<TileTexture, TileBase> tilePairs = new();
    public CancellationTokenSource TokenSource { get; private set; }
    public MapSettings Settings { get; private set; }

    [field: SerializeField] public bool LoadMapInCurrentScene { get; private set; }
    [field: SerializeField] public GameObject MapPrefab { get; private set; }
    [field: SerializeField] public GameObject TravelToNextMapPrefab { get; private set; }
    [field: SerializeField] public int RegionWidth { get; private set; }
    [field: SerializeField] public int RegionHeight { get; private set; }

    public Dictionary<Vector2Int, List<GameObject>> mapRegions;
    private readonly HashSet<Vector2Int> previousRegions = new(), currentRegions = new();

    private Camera cameraReference;
    private Vector2Int cBottomLeft, cTopRight, region;

    [HideInInspector] public Map currentMap, nextMap;

    private bool tryingToLoadMap = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Settings = GetComponent<MapSettings>();
    }

    private void Start()
    {
        TokenSource = new();

        RegionHeight = 16;
        RegionWidth = 16;

        foreach (TilePair pair in tiles)
        {
            tilePairs.Add(pair.type, pair.tile);
        }

        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.LoadSpecifiedData(this);
        }

        MapGeneration.GenerateMapAsync(this, MapPrefab);

        SceneManager.sceneLoaded += OnSceneLoad;
        MapGeneration.OnGenerationCompleted += MapGeneration_OnGenerationCompleted;
        SubscribeToRegionSwitch();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && !tryingToLoadMap)
        {
            StartCoroutine(TryToLoadMap(nextMap));
        }
    }

    public void LoadData(GameData data)
    {
        Settings.seed = data.seed;

        // Rooms
        Settings.totalRoomsCount = data.totalRoomsCount;
        Settings.AmountOfMainRooms = data.amountOfMainRooms;
        Settings.roomMaxSize = data.roomMaxSize;
        Settings.RoomMinSize = data.roomMinSize;

        // Spawn
        Settings.spawnFunction = data.spawnFunction;
        Settings.radiusForGen = data.radiusForGen;
        Settings.stripSizeForGen = data.stripSizeForGen;

        // Hallways
        Settings.amountOfHallwayLoops = data.amountOfHallwayLoops;
        Settings.randomizedHallwayWidth = data.randomizedHallwayWidth;
        Settings.hallwayWidth = data.hallwayWidth;
        Settings.hallwayMinWidth = data.hallwayMinWidth;
        Settings.hallwayMaxWidth = data.hallwayMaxWidth;

        // Enemies
        Settings.amountOfEnemies = data.amountOfEnemies;
    }

    public void SaveData(GameData data)
    {
        data.seed = currentMap.RequiredSettingsForMap.seed;
        
        // Rooms
        data.totalRoomsCount = currentMap.RequiredSettingsForMap.totalRoomsCount;
        data.amountOfMainRooms = currentMap.RequiredSettingsForMap.amountOfMainRooms;
        data.roomMaxSize = currentMap.RequiredSettingsForMap.roomMaxSize;
        data.roomMinSize = currentMap.RequiredSettingsForMap.roomMinSize;
        
        // Spawn
        data.spawnFunction = currentMap.RequiredSettingsForMap.spawnFunction;
        data.radiusForGen = currentMap.RequiredSettingsForMap.radiusForGen;
        data.stripSizeForGen =  currentMap.RequiredSettingsForMap.stripSizeForGen;
        
        // Hallways
        data.amountOfHallwayLoops = currentMap.RequiredSettingsForMap.amountOfHallwayLoops;
        data.randomizedHallwayWidth = currentMap.RequiredSettingsForMap.randomizedHallwayWidth;
        data.hallwayWidth = currentMap.RequiredSettingsForMap.hallwayWidth;
        data.hallwayMinWidth = currentMap.RequiredSettingsForMap.hallwayMinWidth;
        data.hallwayMaxWidth = currentMap.RequiredSettingsForMap.hallwayMaxWidth;
        
        // Enemies
        data.amountOfEnemies = currentMap.RequiredSettingsForMap.amountOfEnemies;
    }

    private void MapGeneration_OnGenerationCompleted(object sender, EventArgs e)
    {
        Settings.seed++;

        if (nextMap == null)
        {
            if (LoadMapInCurrentScene)
            {
                cameraReference = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                StartCoroutine(TryToLoadMap(currentMap));
            }

            MapGeneration.GenerateMapAsync(this, MapPrefab);
        }
    }

    #region Stop Async on disable 
    private void OnDisable()
    {
        if (Instance == this)
        {
            TokenSource.Cancel();
        }
    }
    #endregion

    #region Region switch releated
    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        cameraReference = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        SubscribeToRegionSwitch();
    }

    private void SubscribeToRegionSwitch()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnRegionSwitch -= Instance_OnRegionSwitch;
            Player.Instance.OnRegionSwitch += Instance_OnRegionSwitch;
        }
    }

    #region Game objects
    public GameObject SpawnPrefab(GameObject prefab, Vector3 spawnPosition, Map map, bool activeStatus = true)
    {
        if (map.startRoom.groundTiles.Contains(Vector3Int.RoundToInt(spawnPosition)) && Settings.doNotAllowInStartingRoom.Contains(prefab))
        {
            return null;
        }

        GameObject spawnedPrefab = Instantiate(prefab, spawnPosition + new Vector3(0.5f, 0.5f), Quaternion.identity, map.ActiveGameObjectsParent.transform);
        SetGameObjectsRegion(spawnedPrefab, map.MapRegions);
        spawnedPrefab.SetActive(activeStatus);

        return spawnedPrefab;
    }

    public void SetGameObjectsRegion(GameObject gameObject, Dictionary<Vector2Int, List<GameObject>> regionDictionary)
    {
        Vector2Int region = new((int)(gameObject.transform.position.x / RegionWidth), (int)(gameObject.transform.position.y / RegionHeight));

        if (regionDictionary.ContainsKey(region))
        {
            regionDictionary[region].Add(gameObject);
        }
        else
        {
            regionDictionary.Add(region, new List<GameObject> { gameObject });
        }
    }

    public void RemoveGameObject(GameObject g, Dictionary<Vector2Int, List<GameObject>> regionDictionary)
    {
        Vector2Int region = new((int)(g.transform.position.x / RegionWidth), (int)(g.transform.position.y / RegionHeight));

        if (regionDictionary.TryGetValue(region, out List<GameObject> gameObjects))
        {
            gameObjects.Remove(g);

            if (gameObjects.Count <= 0)
            {
                regionDictionary.Remove(region);
            }
        }

        Destroy(g);
    }
    #endregion
    #endregion

    #region Load map methods
    private void LoadMap(Map map)
    {
        map.gameObject.SetActive(true);

        mapRegions = map.GetComponent<Map>().MapRegions;

        Player.Instance.transform.position = map.startRoom.WorldPosition;
        cameraReference.transform.position = Player.Instance.transform.position;

        UpdateActiveRegions();

        GameObject g1 = new() { name = "Start Room" };
        g1.transform.position = map.startRoom.WorldPosition;

        GameObject g2 = new() { name = "End Room" };
        g2.transform.position = map.endRoom.WorldPosition;
    }

    public IEnumerator TryToLoadMap(Map mapToLoad)
    {
        tryingToLoadMap = true;

        while (Player.Instance == null || mapToLoad == null || !mapToLoad.readyToLoad)
        {
            yield return null;
        }

        if (mapToLoad != currentMap)
        {
            Destroy(currentMap.gameObject);
            currentMap = mapToLoad;
            currentMap.name = "Active Map";
            nextMap = null;
            MapGeneration.GenerateMapAsync(this, MapPrefab);
        }

        LoadMap(mapToLoad);

        tryingToLoadMap = false;
    }

    #endregion

    #region Update active regions
    private void Instance_OnRegionSwitch(object sender, EventArgs e)
    {
        if (currentMap != null && currentMap.gameObject.activeInHierarchy)
        {
            UpdateActiveRegions();
        }
    }

    private void UpdateActiveRegions()
    {
        currentRegions.Clear();

        cBottomLeft = Vector2Int.FloorToInt(0.0625f * cameraReference.ScreenToWorldPoint(new Vector3(0, 0, cameraReference.nearClipPlane)));
        cTopRight = Vector2Int.CeilToInt(0.0625f * cameraReference.ScreenToWorldPoint(new Vector3(cameraReference.pixelWidth, cameraReference.pixelHeight, cameraReference.nearClipPlane)));

        for (int x = cBottomLeft.x; x < cTopRight.x + 1; x++)
        {
            for (int y = cBottomLeft.y; y < cTopRight.y + 1; y++)
            {
                region = new(x, y);

                if (!mapRegions.TryGetValue(region, out var enableList))
                {
                    continue;
                }
                else if (previousRegions.Contains(region))
                {
                    previousRegions.Remove(region);
                    currentRegions.Add(region);
                    continue;
                }

                for (int i = enableList.Count - 1; i >= 0; i--)
                {
                    if (enableList[i] == null)
                    {
                        Debug.Log("Region had no referance to object");
                        mapRegions[region].RemoveAt(i);
                        continue;
                    }

                    enableList[i].SetActive(true);
                }

                currentRegions.Add(region);
            }
        }

        foreach (Vector2Int previousRegion in previousRegions)
        {
            if (mapRegions.TryGetValue(previousRegion, out var disableList))
            {
                for (int i = disableList.Count - 1; i >= 0; i--)
                {
                    if (disableList[i] == null)
                    {
                        Debug.Log("Region had no referance to object");
                        mapRegions[previousRegion].RemoveAt(i);
                        continue;
                    }

                    disableList[i].SetActive(false);
                }
            }
        }

        previousRegions.Clear();
        previousRegions.UnionWith(currentRegions);
    }
    #endregion
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
    walkableIcon
}

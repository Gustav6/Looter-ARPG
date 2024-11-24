using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [BoxGroup("Array of tiles")]
    public TilePair[] tiles;

    public readonly Dictionary<TileTexture, TileBase> tilePairs = new();
    public CancellationTokenSource TokenSource { get; private set; }
    public MapSettings Settings { get; private set; }

    [field: SerializeField] public GameObject MapPrefab { get; private set; }
    [field: SerializeField] public int RegionWidth { get; private set; }
    [field: SerializeField] public int RegionHeight { get; private set; }

    public Dictionary<Vector2Int, List<GameObject>> mapRegions;
    private readonly HashSet<Vector2Int> previousRegions = new(), currentRegions = new();

    private Camera cameraReference;
    private Vector2Int cBottomLeft, cTopRight, region;

    [HideInInspector] public Map currentMap, nextMap;

    private bool tryingToLoadMap = false;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Settings = GetComponent<MapSettings>();

        TokenSource = new();

        RegionHeight = 16;
        RegionWidth = 16;

        foreach (TilePair pair in tiles)
        {
            tilePairs.Add(pair.type, pair.tile);
        }

        MapGeneration.GenerateMapAsync(this, MapPrefab);

        SceneManager.sceneLoaded += OnSceneLoad;
        MapGeneration.OnGenerationCompleted += MapGeneration_OnGenerationCompleted;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && !tryingToLoadMap)
        {
            StartCoroutine(TryToLoadMap(nextMap));
        }
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            TokenSource.Cancel();
        }
    }

    private void MapGeneration_OnGenerationCompleted(object sender, EventArgs e)
    {
        Settings.seed++;

        if (nextMap == null)
        {
            MapGeneration.GenerateMapAsync(this, MapPrefab);
        }
    }

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

        while (Player.Instance == null || mapToLoad == null || !mapToLoad.generationComplete)
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


    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        cameraReference = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        if (Player.Instance != null)
        {
            Player.Instance.OnRegionSwitch -= Instance_OnRegionSwitch;
            Player.Instance.OnRegionSwitch += Instance_OnRegionSwitch;
        }
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
        cTopRight = Vector2Int.FloorToInt(0.0625f * cameraReference.ScreenToWorldPoint(new Vector3(cameraReference.pixelWidth, cameraReference.pixelHeight, cameraReference.nearClipPlane)));

        for (int x = cBottomLeft.x - 1; x < cTopRight.x + 2; x++)
        {
            for (int y = cBottomLeft.y - 1; y < cTopRight.y + 2; y++)
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

                for (int i = 0; i < enableList.Count; i++)
                {
                    enableList[i].SetActive(true);
                }

                currentRegions.Add(region);
            }
        }

        foreach (Vector2Int previousRegion in previousRegions)
        {
            if (mapRegions.TryGetValue(previousRegion, out var disableList))
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

    public GameObject SpawnPrefab(GameObject prefab, Vector3Int tileSpawnPosition, Map map, bool activeStatus = true)
    {
        if (map.startRoom.groundTiles.Contains(tileSpawnPosition) && Settings.doNotAllowInStartingRoom.Contains(prefab))
        {
            return null;
        }

        GameObject spawnedPrefab = Instantiate(prefab, tileSpawnPosition + new Vector3(0.5f, 0.5f), Quaternion.identity, map.ActiveGameObjectsParent.transform);
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

    public void RemoveGameObject(GameObject g, Dictionary<Vector2Int, List<GameObject>> regionDictionary, Vector2Int region)
    {
        if (regionDictionary.TryGetValue(region, out List<GameObject> gameObjects))
        {
            gameObjects.Remove(g);
        }

        Destroy(g);
    }
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

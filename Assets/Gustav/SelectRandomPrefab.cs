using System;
using System.Collections;
using UnityEngine;

public class SelectRandomPrefab : MonoBehaviour
{
    public PrefabChancePair[] pairs;
    public Map mapToSpawnOn;
    public float value;

    void Start()
    {
        StartCoroutine(WaitForMapLoad());
    }

    private IEnumerator WaitForMapLoad()
    {
        while (!MapManager.Instance.currentMap.readyToLoad)
        {
            yield return new WaitForEndOfFrame();
        }

        if (mapToSpawnOn == null)
        {
            yield break;
        }

        GameObject prefabToSpawn = null;
        Vector3 offset = Vector2.zero;

        foreach (PrefabChancePair pair in pairs)
        {
            if (pair.spawnChance >= value)
            {
                prefabToSpawn = pair.prefab;
                offset = pair.offset;
            }
        }

        if (prefabToSpawn != null)
        {
            MapManager.Instance.SpawnPrefab(prefabToSpawn, transform.position - (Vector3)(Vector2.one / 2) + offset, mapToSpawnOn);
            Destroy(this);
        }
    }

    [Serializable]
    public struct PrefabChancePair
    {
        public float spawnChance;
        public GameObject prefab;
        public Vector2 offset;
    }
}

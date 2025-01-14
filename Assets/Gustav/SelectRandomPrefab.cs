using System;
using System.Collections;
using UnityEngine;

public class SelectRandomPrefab : MonoBehaviour
{
    public PrefabChancePair[] pairs;

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

        float value = UnityEngine.Random.Range(0, 100);
        GameObject prfabToSpawn = null;
        Vector3 offset = Vector2.zero;

        foreach (PrefabChancePair pair in pairs)
        {
            if (pair.spawnChance >= value)
            {
                prfabToSpawn = pair.prefab;
                offset = pair.offset;
            }
        }

        if (prfabToSpawn != null)
        {
            Instantiate(prfabToSpawn, transform.position + offset, Quaternion.identity);
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

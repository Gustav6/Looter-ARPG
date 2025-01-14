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

        foreach (PrefabChancePair pair in pairs)
        {
            
        }
    }

    [Serializable]
    public struct PrefabChancePair
    {
        public float chance;
        public GameObject prefab;
    }
}

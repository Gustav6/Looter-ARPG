using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleManager : MonoBehaviour
{
    public static DestructibleManager Instance { get; private set; }

    private List<GameObject> breakble = new();

    public HashSet<Vector3Int> BreakblePositions { get; private set; }

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

        BreakblePositions = new HashSet<Vector3Int>();
    }


    public void AddBreakble(Vector3Int position, GameObject prefab)
    {
        breakble.Add(Instantiate(prefab, position, Quaternion.identity, transform));

        BreakblePositions.Add(position);
    }

    public void DestroyTraps()
    {
        for (int i = 0; i < breakble.Count; i++)
        {
            Destroy(breakble[i]);
        }

        BreakblePositions.Clear();
    }
}

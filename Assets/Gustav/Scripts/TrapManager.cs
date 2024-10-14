using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public static TrapManager Instance { get; private set; }

    private List<GameObject> traps = new();

    public HashSet<Vector3Int> TrapsPositions { get; private set; }

    //[field: SerializeField] public Trap[] trapArray { get; private set; }
    //private Dictionary<TrapType, GameObject> trapPairs = new();

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

        TrapsPositions = new HashSet<Vector3Int>();

        //foreach (Trap trap in trapArray)
        //{
        //    trapPairs.Add(trap.type, trap.prefab);
        //}
    }

    public void AddTrap(Vector3Int position, GameObject prefab)
    {
        traps.Add(Instantiate(prefab, position, Quaternion.identity, transform));

        TrapsPositions.Add(position);
    }

    public void DestroyTraps()
    {
        for (int i = 0; i < traps.Count; i++)
        {
            Destroy(traps[i]);
        }

        TrapsPositions.Clear();
    }
}

//[System.Serializable]
//public struct Trap
//{
//    public TrapType type;
//    public GameObject prefab;
//}

public enum TrapType
{
    spike
}

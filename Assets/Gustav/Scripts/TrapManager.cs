using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public static TrapManager Instance { get; private set; }

    public Renderer test;


    private List<GameObject> traps = new();
    public Dictionary<Room, List<GameObject>> TrapsWithinRoom { get; private set; }

    public HashSet<Vector3Int> TrapsPositions { get; private set; }

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

        TrapsWithinRoom = new();
        TrapsPositions = new HashSet<Vector3Int>();
    }

    public void AddTrap(Vector3Int position, GameObject prefab, Room relevantRoom)
    {
        GameObject trap = Instantiate(prefab, position + (Vector3)(Vector2.one / 2), Quaternion.identity, transform);
        trap.GetComponent<BoxCollider2D>().enabled = false;
        trap.SetActive(false);

        if (TrapsWithinRoom.ContainsKey(relevantRoom))
        {
            TrapsWithinRoom[relevantRoom].Add(trap);
        }
        else
        {
            TrapsWithinRoom.Add(relevantRoom, new List<GameObject>() { trap });
        }

        traps.Add(trap);

        TrapsPositions.Add(position);
    }

    public void EnablePrefabs(Room room)
    {
        foreach (GameObject trap in TrapsWithinRoom[room])
        {
            trap.SetActive(true);
        }
    }

    public void ClearTraps()
    {
        for (int i = 0; i < traps.Count; i++)
        {
            Destroy(traps[i]);
        }

        TrapsWithinRoom.Clear();
        TrapsPositions.Clear();
    }
}


using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public static TrapManager Instance { get; private set; }

    private List<GameObject> traps = new();
    public Dictionary<Room, List<GameObject>> TrapsWithinRoom { get; private set; }

    public HashSet<Vector3Int> TrapsPositions { get; private set; }

    private List<GameObject> parentObjects = new();

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
        TrapsPositions = new();
    }

    public void AddTrap(Vector3Int position, GameObject prefab, Room relevantRoom)
    {
        GameObject trap = Instantiate(prefab, position + (Vector3)(Vector2.one / 2), Quaternion.identity, transform);
        trap.GetComponent<BoxCollider2D>().enabled = false;

        if (TrapsWithinRoom.ContainsKey(relevantRoom))
        {
            trap.transform.parent = TrapsWithinRoom[relevantRoom].First().transform.parent;

            TrapsWithinRoom[relevantRoom].Add(trap);
        }
        else
        {
            GameObject parent = new()
            {
                name = "Room " + TrapsWithinRoom.Count
            };

            parentObjects.Add(parent);
            parent.transform.parent = transform;
            trap.transform.parent = parent.transform;

            TrapsWithinRoom.Add(relevantRoom, new List<GameObject>() { trap });

            parent.SetActive(false);
        }

        traps.Add(trap);

        TrapsPositions.Add(position);
    }

    public void EnablePrefabs(Room room)
    {
        TrapsWithinRoom[room].First().transform.parent.gameObject.SetActive(true);
    }

    public void ClearTraps()
    {
        foreach (GameObject gameObject in parentObjects)
        {
            Destroy(gameObject);
        }

        parentObjects.Clear();
        TrapsPositions.Clear();
        TrapsWithinRoom = new();
    }
}


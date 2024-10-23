using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DestructibleManager : MonoBehaviour
{
    public static DestructibleManager Instance { get; private set; }

    private List<GameObject> breakables = new();

    public Dictionary<Room, List<GameObject>> BreakablesWithinRoom { get; private set; }

    public HashSet<Vector3Int> BreakablePositions { get; private set; }

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

        BreakablesWithinRoom = new();
        BreakablePositions = new();
    }

    public void AddBreakable(Vector3Int position, GameObject prefab, Room relevantRoom)
    {
        GameObject breakable = Instantiate(prefab, position + (Vector3)(Vector2.one / 2), Quaternion.identity, transform);
        breakable.GetComponent<BoxCollider2D>().enabled = false;

        if (BreakablesWithinRoom.ContainsKey(relevantRoom))
        {
            breakable.transform.parent = BreakablesWithinRoom[relevantRoom].First().transform.parent;

            BreakablesWithinRoom[relevantRoom].Add(breakable);
        }
        else
        {
            GameObject parent = new()
            {
                name = "Room " + BreakablesWithinRoom.Count
            };

            parentObjects.Add(parent);
            parent.transform.parent = transform;
            breakable.transform.parent = parent.transform;

            BreakablesWithinRoom.Add(relevantRoom, new List<GameObject>() { breakable });

            parent.SetActive(false);
        }

        breakables.Add(breakable);

        BreakablePositions.Add(position);
    }

    public void EnablePrefabs(Room room)
    {
        BreakablesWithinRoom[room].First().transform.parent.gameObject.SetActive(true);
    }

    public void ClearBreakables()
    {
        foreach (GameObject gameObject in parentObjects)
        {
            Destroy(gameObject);
        }

        parentObjects.Clear();
        BreakablePositions.Clear();
        BreakablesWithinRoom = new();
    }
}

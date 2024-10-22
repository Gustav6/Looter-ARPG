using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DestructibleManager : MonoBehaviour
{
    public static DestructibleManager Instance { get; private set; }

    private List<GameObject> breakbles = new();

    public Dictionary<Room, List<GameObject>> BreakablesWithinRoom { get; private set; }

    public HashSet<Vector3Int> BreakablePositions { get; private set; }

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
        breakable.SetActive(false);

        if (BreakablesWithinRoom.ContainsKey(relevantRoom))
        {
            BreakablesWithinRoom[relevantRoom].Add(breakable);
        }
        else
        {
            BreakablesWithinRoom.Add(relevantRoom, new List<GameObject>() { breakable });
        }

        breakbles.Add(breakable);

        BreakablePositions.Add(position);
    }

    public void EnablePrefabs(Room room)
    {
        foreach (GameObject breakable in BreakablesWithinRoom[room])
        {
            breakable.SetActive(true);
        }
    }

    public void ClearBreakbles()
    {
        for (int i = 0; i < breakbles.Count; i++)
        {
            Destroy(breakbles[i]);
        }

        BreakablesWithinRoom.Clear();
        BreakablePositions.Clear();
    }
}

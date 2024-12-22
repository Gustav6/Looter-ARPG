using UnityEngine;

public class TravelToNextMap : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        StartCoroutine(MapManager.Instance.TryToLoadMap(MapManager.Instance.nextMap));
        Debug.Log("Travel To Next Map!!");
    }
}

using System.Collections;
using UnityEngine;

public class TileRemoverTest : MonoBehaviour
{
    public Vector3Int position;

    private void Start()
    {
        position = MapManager.Instance.currentMap.WallMap.WorldToCell(transform.position);

        StartCoroutine(WaitForMapLoad());
    }

    private IEnumerator WaitForMapLoad()
    {
        while (!MapManager.Instance.currentMap.readyToLoad)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log(position);
        //MapManager.Instance.currentMap.WallMap.SetTile(pos, null);
    }

    private void OnDestroy()
    {
        //Debug.Log("Destroyed");
    }
}

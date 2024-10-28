using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    [field: SerializeField] public int Health { get; private set; }

    [SerializeField] private BoxCollider2D colliderComponent;
    [SerializeField] private Rigidbody2D rbComponent;

    private void OnBecameVisible()
    {
        colliderComponent.enabled = true;
        rbComponent.simulated = true;
    }

    private void OnBecameInvisible()
    {
        colliderComponent.enabled = false;
        rbComponent.simulated = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector2Int region = new((int)(transform.position.x / MapManager.Instance.RegionWidth), (int)(transform.position.y / MapManager.Instance.RegionHeight));
            MapManager.Instance.regions[region].Remove(gameObject);
            Destroy(gameObject);
        }
    }
}

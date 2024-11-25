using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootScript : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D boxCollider;
    private Vector2 moveDirection;

    private void Start()
    {
        moveDirection = new (Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        rb.AddForce(moveDirection * 300, ForceMode2D.Impulse);
    }

    private void Update()
    {
        rb.linearVelocity *= 0.95f;

        if (Mathf.Abs(rb.linearVelocity.x) <= 0.1f && Mathf.Abs(rb.linearVelocity.y) <= 0.1f)
        {
            rb.linearVelocity = Vector2.zero;

            boxCollider.isTrigger = true;
            rb.excludeLayers = new();

            GetComponent<LootScript>().enabled = false;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Picked up loot");
            MapManager.Instance.RemoveGameObject(gameObject, MapManager.Instance.currentMap.MapRegions);
        }
    }

    private void OnBecameInvisible()
    {
        TransitionSystem.transitionPairs.Remove(gameObject);
    }

    private void OnBecameVisible()
    {
        TransitionSystem.AddTransition(new ScaleTransition(transform, 1, CurveType.SinCurve, 2, 0.05f, Vector2.one), gameObject, true, true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootScript : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    private void Start()
    {
        Vector2 t = new (Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        rb.AddForce(t * 150, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Picked up loot");
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

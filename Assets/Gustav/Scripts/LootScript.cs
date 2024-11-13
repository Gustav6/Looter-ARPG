using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootScript : MonoBehaviour
{
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

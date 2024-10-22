using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public Animator animator;
    [field: SerializeField] public int Damage { get; private set; }

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
}

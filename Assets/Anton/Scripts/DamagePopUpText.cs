using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopUpText : MonoBehaviour
{
    public TextMeshPro textMeshPro;
    float timer;
    public float destructTimer;
    public const float moveSpeed = 35;

    Rigidbody2D rb;

    void Start()
    {
        timer = destructTimer;

        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(Random.Range(-0.2f, 0.2f), 0.7f) * moveSpeed;

        TransitionSystem.AddTransition(new ScaleTransition(transform, 0.5f, new Vector3(1.5f, 1.5f), TransitionType.SmoothStop2, ShrinkAfterFirstTransiton), gameObject); 
    }

    private void Update()
    {
        rb.linearVelocity -= new Vector2(0, 0.3f);
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void ShrinkAfterFirstTransiton()
    {     
       TransitionSystem.AddTransition(new ScaleTransition(transform, 0.5f, new Vector3(.1f, .1f), TransitionType.SmoothStart2), gameObject);
    }
}
    
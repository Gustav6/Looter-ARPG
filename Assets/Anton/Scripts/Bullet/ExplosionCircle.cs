using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionCircle : MonoBehaviour
{
    float timer;
    private void Start()
    {
        transform.localScale = new Vector3(1, 1);
    }
    void FixedUpdate()
    {
        transform.localScale += new Vector3(2f, 2f);
        timer += Time.deltaTime;

        if (timer >= 0.2)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            return;
        }

        if (collision.TryGetComponent<IDamagable>(out IDamagable damagable))
        {
            damagable.Damage(10);
        }
    }
}

using UnityEngine;

public class Cactus : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        if (collision.TryGetComponent(out IDamagable d))
        {
            if (collision.TryGetComponent(out Controller2D c))
            {
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;

                d.Knockback(c, knockbackDirection, 0.2f);
            }
        }
    }
}

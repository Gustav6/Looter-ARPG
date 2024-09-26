using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntonsTemporaryEnemyScript : MonoBehaviour
{
    public GameObject damagePopupPrefab;   
    public int maxhealth = 100;
    private int currenthealth;

    private void Start()
    {
        currenthealth = maxhealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Bullet")
        {
            TakeDamage(GunController.Damage);          
        }
    }

    public void TakeDamage(int damage)
    {
        currenthealth -= damage;
        Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);

        if (currenthealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}

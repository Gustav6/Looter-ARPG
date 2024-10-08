using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            if (collision.tag == "Fire")
            {
              StartCoroutine("DmgOverTime");
            }    
                 
            if (collision.tag == "Bullet")
            {
                TakeDamage(GunController.Damage);
            }

            if (collision.tag == "Explosion")
            {
                TakeDamage(GunController.Damage / 2);
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

    IEnumerable DmgOverTime(int damage)
    {
        for (int i = 0; i < 10; i++)
        {
            currenthealth -= damage / 5;
            Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);

            if (currenthealth <= 0)
            {
                Destroy(gameObject);
            }

            StartCoroutine(LoopDelay());
            
        }
        yield return null;
    }

    IEnumerator LoopDelay()
    {
        yield return new WaitForSeconds(60);  
    }
}

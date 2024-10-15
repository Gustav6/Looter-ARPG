using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AntonsTemporaryEnemyScript : MonoBehaviour
{
    public GameObject damagePopupPrefab;   
    public int maxhealth = 100;
    private int currenthealth;
    private bool startTimer;
    private float timer;
    private int dmgTickCounter;

    public static int dmgTakenEnemy;

    private void Start()
    {
        currenthealth = maxhealth;
    }

    private void Update()
    {
        if (startTimer)
        {
            timer += Time.deltaTime;
            if (timer >= 1)
            {
                TakeDamage(GunController.Damage / 5);
                timer = 0;
                dmgTickCounter += 1;

                if (dmgTickCounter >= 10)
                {
                    startTimer = false;
                    dmgTickCounter = 0;
                }
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
            if (collision.tag == "Fire")
            {
              startTimer = true;
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
        dmgTakenEnemy = damage;
        Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);

        if (currenthealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealthScript : MonoBehaviour
{

    public int maxHealth = 100;
    public int currentHealth;
    public float healthAmount = 100f;


    public HealthBar healthBar;

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(20);
        }

           
       if (Input.GetKeyDown(KeyCode.Delete))
            {
                HealPlayer(10);
            }

        

    }

   

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Spikes")
        {
            TakeDamage(25);
        }
    }



    void TakeDamage(int damage)
    {
        currentHealth -= damage;

        healthBar.SetHealth(currentHealth);
    }
    void HealPlayer(int heal)
    {
        {
            currentHealth += heal;

            healthBar.SetHealth(currentHealth);
        }
    }
 public void Respawn()
    {
        if (currentHealth <= 0)
        {
            SceneManager.LoadScene("My Test Scene");
        }
    }
 



}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealthScript : MonoBehaviour
{
    public FloatVariable health, maxHealth;


    public void Start()
    {
        health.value = 100f;
    }
    public void TakeDamage(float amount)
    {
        health.value -= amount;
        if (health.value <= 0)
        {
            health.value = 0;
            Debug.Log("You're dead");
        }
    }
    public void Update()
    {
        if (health.value >= 100)
        {
            health.value = 100;
            Debug.Log("Full Health");
        }
    }

    public void Heal(float amount)
    {
        health.value += amount;
        if (health.value <= 100)
        {
            health.value = 100;
            Debug.Log("Full HP");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Spikes")
        {
            TakeDamage(12);
        }
    }
    
}

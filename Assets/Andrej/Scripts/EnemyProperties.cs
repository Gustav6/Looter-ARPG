using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProperties : MonoBehaviour
{
    public GameObject player;
    public float distanceToPlayer;

    public float speed = 10f;
    public float health = 100f;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (health <= 0)
        {
            Destroy(this.gameObject);
        }

        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
    }
}

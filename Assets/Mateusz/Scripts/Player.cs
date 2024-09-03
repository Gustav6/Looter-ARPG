using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    float moveSpeed = 6;

    Vector3 velocity;
    Vector2 direction;

    Controller2D controller;



    private void Start()
    {
        controller = GetComponent<Controller2D>();
    }

    private void Update()
    {
        direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        velocity = direction.normalized * moveSpeed;
        controller.Move(velocity * Time.deltaTime);

    }
}
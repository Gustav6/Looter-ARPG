using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntonsPlayerController : MonoBehaviour
{
    Rigidbody2D rb;

    public static float walkSpeed = 15f;
    float speedLimiter = 0.7f;
    float inputHorizontal;
    float inputVertical;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        walkSpeed = 15f;
    }

    private void Update()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        if (inputHorizontal != 0 || inputVertical != 0)
        {
            if (inputHorizontal != 0 && inputVertical != 0)
            {
                inputHorizontal *= speedLimiter;
                inputVertical *= speedLimiter;
            }

            rb.velocity = new Vector2(inputHorizontal * walkSpeed, inputVertical * walkSpeed);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
}
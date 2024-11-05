using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntonsPlayerController : MonoBehaviour
{
    Rigidbody2D rb;

    public static float walkSpeed = 25f;
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

            rb.linearVelocity = new Vector2(inputHorizontal * walkSpeed, inputVertical * walkSpeed);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}

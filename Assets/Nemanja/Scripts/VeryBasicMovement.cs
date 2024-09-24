using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeryBasicMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed = 5f;


    private void Start()
    {
        
    }
    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);

        transform.Translate(direction * _speed * Time.deltaTime);
    }
}

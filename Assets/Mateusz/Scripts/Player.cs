using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    float walkSpeed = 5;
    float runSpeed = 25f;
   public float currentSpeed;
   public float stamina, staminaMax;
   public float runCost;
   public   float chargeRate;

    bool running;


    public Image staminaBar;

    private Coroutine recharge;
    

    Vector3 velocity;

    Controller2D controller;



    private void Start()
    {
        controller = GetComponent<Controller2D>();
        Debug.Log(+currentSpeed);
    }

    private void Update()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        currentSpeed = walkSpeed;

        velocity.x = direction.x * currentSpeed;
        velocity.y = direction.y * currentSpeed;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftShift) && stamina > 0)
        {
            running = true;

        }else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            running = false;
        }

        if (running)
        {
            currentSpeed = runSpeed;
            stamina -= runCost * Time.deltaTime;
            if (stamina < 0) 
            {
                stamina = 0;
                staminaBar.fillAmount = stamina / staminaMax;
            }
            StopCoroutine(recharge);
        }
        else if (!running)
        {
            currentSpeed = walkSpeed;
            recharge = StartCoroutine(rechargeStamina());
        }
    }

    private IEnumerator rechargeStamina()
    {
        yield return new WaitForSeconds(1f);

        while (stamina < staminaMax)
        {
            stamina += chargeRate / 10f;
            if (stamina > staminaMax)
            {
                stamina = staminaMax;
                staminaBar.fillAmount = stamina / staminaMax;
                yield return new WaitForSeconds(.1f);
            }
        }
    }
}
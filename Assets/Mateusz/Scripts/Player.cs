using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public float moveSpeed;
    [SerializeField]
    private float stamina;
    public float maxStamina = 100;
    public float runCost = 25f;
    public float rechargeTime = 1f;

    public InventoryObject inventory;

    [SerializeField]
    private Image staminaProgressUI = null;
    [SerializeField]
    private CanvasGroup sliderCanvasGroup = null;

    public GameObject inventoryCanvas;

    bool inventoryOpen;
    bool running;

    Vector3 velocity;

    Controller2D controller;



    private void Start()
    {
        stamina = maxStamina;
        moveSpeed = 5;
        controller = GetComponent<Controller2D>();
    }

    private void Update()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));


        velocity.x = direction.x * moveSpeed;
        velocity.y = direction.y * moveSpeed;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftShift) && stamina > 0)
        { 
            running = true;

        }else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            running = false;
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !inventoryOpen)
        {
            inventoryCanvas.SetActive(true);
            inventoryOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.Tab) && inventoryOpen)
        {
            inventoryCanvas.SetActive(false);
            inventoryOpen = false;
        }

        if (running)
        {
            moveSpeed = 10f;
            stamina -= runCost * Time.deltaTime;
            if (stamina < 0)
            {
                running = false;
            }
            UpdateStamina(1);
        }
        else if (!running && stamina < maxStamina)
        {
            moveSpeed = 5f;
            stamina += Time.deltaTime * rechargeTime;
            UpdateStamina(1);
        }

    }


    void UpdateStamina(int value)
    {
        staminaProgressUI.fillAmount = stamina / maxStamina;
        if (value == 0)
        {
            sliderCanvasGroup.alpha = 0;
        }
        else
        {
            sliderCanvasGroup.alpha = 1;
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("henlo");
        var item = other.GetComponent<Item>();
        if (item)
        {
            Debug.Log("henlo2");
            inventory.AddItem(item.item, 1);
            Destroy(other.gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        inventory.Container.Clear();
    }
}
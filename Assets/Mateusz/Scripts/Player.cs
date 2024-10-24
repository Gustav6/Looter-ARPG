using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public bool IsMoving { get; private set; }
    public Vector3 Direction { get; private set; }

    public float moveSpeed;

    [SerializeField] private float currentStamina;
    public float Stamina
    {
        get { return currentStamina; }
        set
        {
            if (value > maxStamina)
            {
                currentStamina = maxStamina;
            }
            else if (value < 0)
            {
                currentStamina = 0;
            }
            else
            {
                currentStamina = value;
            }
        }
    }

    public float maxStamina = 100f;
    public float sprintCost = 25f;
    public float rechargeTime = 1f;

    public InventoryObject inventory;

    [SerializeField]
    private Image staminaProgressUI = null;
    [SerializeField]
    private CanvasGroup sliderCanvasGroup = null;

    public GameObject inventoryCanvas;

    bool inventoryOpen;
    bool sprinting;

    Controller2D controller;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }

        Stamina = maxStamina;
        moveSpeed = 5;
        controller = GetComponent<Controller2D>();
    }

    private void Update()
    {
        IsMoving = false;
        Direction = new (Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            moveSpeed = 10f;
            sprinting = true;

        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Stamina == 0)
        {
            moveSpeed = 5f;
            sprinting = false;
        }

        if (Direction != Vector3.zero)
        {
            IsMoving = true;
            controller.Move(moveSpeed * Time.deltaTime * Direction);
        }

        if (sprinting)
        {
            Stamina -= sprintCost * Time.deltaTime;
            UpdateStaminaBar(1);
        }
        else if (!sprinting && Stamina < maxStamina)
        {
            Stamina += Time.deltaTime * rechargeTime;
            UpdateStaminaBar(1);
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Saving");
            inventory.Save();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Loading");
            inventory.Load();
        }
    }


    void UpdateStaminaBar(int value)
    {
        if (staminaProgressUI != null && sliderCanvasGroup != null)
        {
            staminaProgressUI.fillAmount = Stamina / maxStamina;
            if (value == 0)
            {
                sliderCanvasGroup.alpha = 0;
            }
            else
            {
                sliderCanvasGroup.alpha = 1;
            }
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
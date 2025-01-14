
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour, IDamagable, IDataPersistence
{
    public static Player Instance { get; private set; }

    #region Movement
    public Vector3 InputDirection { get; private set; }

    //[BoxGroup("Movement")]
    [SerializeField] private float currentMoveSpeed;

    //[BoxGroup("Movement")]
    public float sprintAcceleration = 0.15f;
    //[BoxGroup("Movement")]
    public float sprintMoveSpeed = 10;
    //[BoxGroup("Movement")]
    public float baseMoveSpeed = 5;

    private bool sprinting;
    #endregion

    #region Stamina
    //[BoxGroup("Stamina")]
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

            UpdateStaminaBar();
        }
    }
    //[BoxGroup("Stamina")]
    public float maxStamina = 100f;
    //[BoxGroup("Stamina")]
    public float sprintCost = 25f;
    //[BoxGroup("Stamina")]
    public float rechargeTime = 1f; 

    //[BoxGroup("Stamina")]
    [SerializeField] private Image staminaProgressUI = null;
    #endregion

    #region Invenroty
    //[BoxGroup("Inventory")]
    public InventoryObject inventory;

    //[BoxGroup("Inventory")]
    public GameObject inventoryCanvas;

    private bool inventoryOpen;
    #endregion

    #region Health
    [SerializeField] private int health;
    [field: SerializeField] public int MaxHealth { get; set; }

    public int CurrentHealth
    {
        get => health;
        set
        {
            if (value > MaxHealth)
            {
                health = MaxHealth;
            }
            else if (value <= 0)
            {
                OnDeath();
            }
            else
            {
                health = value;
            }
        }
    }
    public bool TickDamageActive { get; set; }
    #endregion

    Controller2D controller;

    [HideInInspector] public bool facingRight = false;
    [field: SerializeField] public Transform SpriteTransform { get; private set; }

    public Vector2Int CurrentRegion { get; private set; }
    public Vector2Int PreviousRegion { get; private set; }

    [field: SerializeField] public bool IsBeingKnockedBack { get; set; }
    [field: SerializeField] public AnimationCurve KnockbackForceCurve { get; set; }
    public Coroutine KnockbackCoroutine { get; set; }

    public event EventHandler OnRegionSwitch;

    [SerializeField] private Renderer groundRenderer;

    private void Awake()
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
        currentMoveSpeed = baseMoveSpeed;
        controller = GetComponent<Controller2D>();

        CurrentHealth = MaxHealth;
    }

    private void Start()
    {
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.LoadSpecifiedData(this);
        }
    }

    private void FixedUpdate()
    {
        if (!IsBeingKnockedBack)
        {
            controller.Move(currentMoveSpeed * Time.fixedDeltaTime * InputDirection);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (this is IDamagable d)
            {
                d.Knockback(controller, new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized, 0.25f);
            }
        }

        if (!IsBeingKnockedBack)
        {
            InputDirection = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            sprinting = true;

        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Stamina == 0)
        {
            sprinting = false;
        }

        if (InputDirection != Vector3.zero || IsBeingKnockedBack)
        {
            groundRenderer.material.mainTextureOffset = transform.position * new Vector2(0.2f, 0.2f);

            if (sprinting)

            {
                currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, sprintMoveSpeed, sprintAcceleration);
                Stamina -= sprintCost * Time.deltaTime;
            }
            else
            {
                currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, baseMoveSpeed, 0.1f);
            }

            if (MapManager.Instance != null)
            {
                if (CurrentRegion != Vector2Int.FloorToInt(new Vector2(transform.position.x / (MapManager.Instance.RegionWidth * 0.5f), transform.position.y / (MapManager.Instance.RegionHeight * 0.5f))))
                {
                    UpdateRegion();
                }
            }
        }

        if(!sprinting || InputDirection == Vector3.zero)
        {
            if (Stamina < maxStamina)
            {
                Stamina += Time.deltaTime * rechargeTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryCanvas != null)
            {
                inventoryOpen = !inventoryOpen;
                inventoryCanvas.SetActive(inventoryOpen);
            }
        }

        if (inventory != null)
        {
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
    }

    public void LoadData(GameData data)
    {
        MaxHealth = data.playerMaxHealth;
        CurrentHealth = data.playerCurrentHealth;
    }

    public void SaveData(GameData data)
    {
        data.playerMaxHealth = MaxHealth;
        data.playerCurrentHealth = CurrentHealth;
    }

    public void UpdateRegion()
    {
        CurrentRegion = new Vector2Int((int)(transform.position.x / MapManager.Instance.RegionWidth), (int)(transform.position.y / MapManager.Instance.RegionHeight));
        OnRegionSwitch?.Invoke(this, EventArgs.Empty);
        PreviousRegion = CurrentRegion;
    }

    void UpdateStaminaBar()
    {
        if (staminaProgressUI != null)
        {
            staminaProgressUI.fillAmount = Stamina / maxStamina;
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("henlo");
        var item = other.GetComponent<GroundItem>();
        if (item)
        {
            Item _item = new Item(item.item);
            Debug.Log("henlo2");
            inventory.AddItem(new Item(item.item), 1);
            Destroy(other.gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        if (inventory != null)
        {
            inventory.Container.Items = new InventorySlot[28];
        }
    }

    public void Damage(int damageAmount)
    {
        CurrentHealth -= damageAmount;
    }

    public void OnDeath()
    {
        Destroy(gameObject);
    }
}
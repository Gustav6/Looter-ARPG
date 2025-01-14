using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public static GunController Instance { get; private set; }

    public GameObject damagePopupPrefab;
    public ScriptableObjectsGuns gun;
    public Transform firePoint;
    public Transform firePoint2;
    public SpriteRenderer sprite;
    public LayerMask collidableLayersRaycast;
    public TextMeshPro ammoCount;
    public Transform weaponRotateAxis;
    int ammo;

    public int Damage;
    public int pierceAmount;
    public bool pierce;
    public bool explosion;
    public bool dmgOverTime;
    public float fireForce;
    public bool fireDmg;

    private float attackTimer;
    float reloadTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Damage = gun.damage;
        attackTimer = gun.fireRate;
        sprite.sprite = gun.sprite;
        ammo = gun.Ammo;
        pierceAmount = gun.amountOfPircableEnemies;
        fireForce = gun.fireForce;
        reloadTimer = 0;

        #region Set Static Variables
        pierce = false;
        explosion = false;
        dmgOverTime = false;
        fireDmg = false;
        if (gun.effects != null && gun.effects.Length != 0)
        {
            if (gun.effects.Contains(WeaponEffect.pierceShot))
            {
                pierce = true;
                Projectile.amountOfPiercableObjects = gun.amountOfPircableEnemies;
            } 

            if (gun.effects.Contains(WeaponEffect.expolsiveOnImpact))
            {
                explosion = true;
            }

            if (gun.effects.Contains(WeaponEffect.dmgOverTime))
            {
                dmgOverTime = true;
            }

            if (gun.damageTypes.Contains(DmgType.fire))
            {
                fireDmg = true;
            }
        }
        #endregion
    }

    void Update()
    {
        #region Input
        if (attackTimer >= gun.fireRate)
        {
            if (Input.GetMouseButton(0))
            {
                RaycastHit2D raycastHit = Physics2D.Raycast(weaponRotateAxis.transform.position, WeaponRotate.dir, 3.2f, collidableLayersRaycast);
                Debug.DrawRay(weaponRotateAxis.transform.position, WeaponRotate.dir.normalized * 3.2f, Color.red);
                if (raycastHit)
                {
                    //nothing happens
                }
                else if (ammo > 0)
                {
                    Attack();
                }
            }
        }
        else if (attackTimer >= gun.fireRate / 4)
        {
            CameraShake.StopShakeCamera();
        }
       

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }

        if (ammo <= 0)
        {
            Reload();
        }
        #endregion

        if (attackTimer < gun.fireRate)
        {
            attackTimer += Time.deltaTime;
        }
    }

    void Attack()
    {
        CameraShake.ShakeCamera(2);
        GameObject bullet = Instantiate(gun.bulletPrefab, firePoint.transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().AddForce(firePoint.right * gun.fireForce, ForceMode2D.Impulse);      
        if (gun.effects != null && gun.effects.Length != 0)
        {
            if (gun.effects.Contains(WeaponEffect.dubbelShot))
            {
                GameObject bullet2 = Instantiate(gun.bulletPrefab, firePoint2.transform.position, Quaternion.identity);
                bullet2.GetComponent<Rigidbody2D>().AddForce(firePoint.right * gun.fireForce, ForceMode2D.Impulse);
            }
        }
        ammo -= 1;
        attackTimer = 0;
    }

    void Reload()
    {
        reloadTimer += Time.deltaTime;
        if (reloadTimer >= gun.reloadTime)
        {
            ammo = gun.Ammo;
            reloadTimer = 0;
        } 
    }
}

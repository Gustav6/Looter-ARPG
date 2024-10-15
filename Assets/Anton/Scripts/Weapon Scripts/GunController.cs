using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] public ScriptableObjectsGuns gun;
    public Transform firePoint;
    public Transform firePoint2;
    public SpriteRenderer sprite;
    public LayerMask enemyLayers;
    public TextMeshPro ammoCount;
    int ammo;
    float timer;
    #region Public Static Variables
    public static int Damage;
    public static int pierceAmount;
    public static bool pierce;
    public static bool explosion;
    public static bool dmgOverTime;
    public static float fireForce;
    #endregion

    private float attackTimer;
    void Start()
    {
        Damage = gun.damage;
        attackTimer = gun.fireRate;
        sprite.sprite = gun.sprite;
        ammo = gun.Ammo;
        pierceAmount = gun.amountOfPircableEnemies;
        fireForce = gun.fireForce;

        #region Set Static Variables
        pierce = false;
        explosion = false;
        dmgOverTime = false;
        if (gun.effects != null && gun.effects.Length != 0)
        {
            if (gun.effects.Contains(WeaponEffect.pierceShot))
            {
                pierce = true;
            }

            if (gun.effects.Contains(WeaponEffect.expolsiveOnImpact))
            {
                explosion = true;
            }

            if (gun.effects.Contains(WeaponEffect.dmgOverTime))
            {
                dmgOverTime = true;
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
                if (ammo > 0)
                {
                    Attack();
                    attackTimer = 0;                 
                }              
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
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
    }

    void Reload()
    {       
        ammo = gun.Ammo;        
        Debug.Log("Reloaded Gun");       
    }
}

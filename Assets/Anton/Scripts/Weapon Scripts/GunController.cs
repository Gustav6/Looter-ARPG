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

    [SerializeField] private bool realodingGun = false;

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
            if (Input.GetMouseButton(0) && !realodingGun)
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
            //FixWeaponPos();
        }
        else if (attackTimer >= gun.fireRate)
        {
            CameraShake.StopShakeCamera();
        }

        if (!realodingGun)
        {
            if (ammo <= 0)
            {
                TestFlash();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                TestFlash();
            }
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
        StartRecoil();
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

    private void StartRecoil()
    {
        Vector2 test = (Player.Instance.transform.position - transform.position).normalized;
        Vector2 test2 = (transform.position - Player.Instance.transform.position).normalized;

        float animationTime;
        float animationForce = fireForce/10;

        if (gun.fireRate >= 0.7f)
        {
           animationTime = 0.6f;
           animationTime /= animationForce;
        }
        else
        {
            animationTime = gun.fireRate;
            animationTime /= animationForce;
        }

        TransitionSystem.AddTransition(new MoveTransition(transform, animationTime, TransitionType.SinCurve, -1, new Vector2(1, 0), new Vector2(1.79f, 0), false, FixWeaponPos), gameObject);     
    }

    private void FixWeaponPos()
    {
        transform.localPosition = new Vector3(1.79f, 0, 0);
        CameraShake.StopShakeCamera();
    }

    private void TestFlash()
    {
        realodingGun = true;
        TransitionSystem.AddTransition(new ColorTransition(transform, gun.reloadTime * 0.075f, new Color(1, 1, 1, 0.75f), TransitionType.SmoothStop2, Reload), gameObject);
    }

    private void Reload()
    {       
        TransitionSystem.AddTransition(new ColorTransition(transform, gun.reloadTime * 0.075f, new Color(1, 1, 1, 1), TransitionType.SmoothStart2), gameObject);
        TransitionSystem.AddTransition(new RotationTransition(transform, gun.reloadTime, new Vector3(0, 0, 360), TransitionType.SmoothStop4, ResetAmmo), gameObject);
    }

    private void ResetAmmo()
    {
        ammo = gun.Ammo;
        realodingGun = false;
    }
}

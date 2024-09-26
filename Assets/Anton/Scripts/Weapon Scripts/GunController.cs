using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] public ScriptableObjectsGuns gun;
    public Transform firePoint;
    public Transform firePoint2;
    public SpriteRenderer sprite;
    public LayerMask enemyLayers;
    public static int Damage;
    public static bool pierce;

    private int attackTimer;
    void Start()
    {
        Damage = gun.damage;
        attackTimer = gun.fireRate;
        sprite.sprite = gun.sprite;
        pierce = gun.piercingShot;

        if (gun.bigBullet)
        {
            firePoint.transform.position -= new Vector3(0, 3, 0);
            firePoint2.transform.position += new Vector3(0, 3, 0);
        }
    }

    void Update()
    {
        if (attackTimer >= gun.fireRate)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
                attackTimer = 0;
            }
        }

        if (attackTimer < gun.fireRate)
        {
            attackTimer += 1;
        }
    }

    void Attack()
    {
        GameObject bullet = Instantiate(gun.bulletPrefab, firePoint.transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().AddForce(firePoint.right * gun.fireForce, ForceMode2D.Impulse);

        if (gun.dubbelShot)
        {
            GameObject bullet2 = Instantiate(gun.bulletPrefab, firePoint2.transform.position, Quaternion.identity);
            bullet2.GetComponent<Rigidbody2D>().AddForce(firePoint.right * gun.fireForce, ForceMode2D.Impulse);
        }
    }

}

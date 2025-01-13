using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    //[SerializeField] public ScriptableObjectsWeapons weapon;
    //public Transform attackPoint;
    //public SpriteRenderer sprite;
    //public LayerMask enemyLayers;
    //public static int Damage;

    //private int attackTimer;
    //void Start()        
    //{
    //    Damage = weapon.damage;
    //    attackTimer = weapon.attackSpeed;
    //    sprite.sprite = weapon.sprite;
    //}

    //void Update()
    //{
    //    if (attackTimer >= weapon.attackSpeed)
    //    {
    //        if (Input.GetMouseButtonDown(0))
    //        {
    //            Attack();
    //            attackTimer = 0;
    //        } 
    //    }

    //    if (attackTimer < weapon.attackSpeed)
    //    {
    //        attackTimer += 1;
    //    }    
    //}

    //void Attack()
    //{
    //    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, weapon.range, enemyLayers);

    //    foreach (var enemy in hitEnemies)
    //    {
    //        enemy.GetComponent<AntonsTemporaryEnemyScript>().TakeDamage(weapon.damage);
    //    }
    //}

    //private void OnDrawGizmosSelected()
    //{
    //    if (attackPoint == null)
    //    {
    //        return;
    //    }
    //    Gizmos.DrawWireSphere(attackPoint.position, weapon.range);
    //}
}

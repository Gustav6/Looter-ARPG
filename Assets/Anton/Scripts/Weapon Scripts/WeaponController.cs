using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TreeEditor;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] public ScriptableObjectsWeapons weapon;
    public CircleCollider2D damageHitbox;

    public Transform sword;
    private int attackTimer;

    void Start()
    {
        attackTimer = weapon.attackSpeed;
        damageHitbox.radius = weapon.range;
    }

    void Update()
    {
        Attack();
    }

    void Attack()
    {
        if (attackTimer >= weapon.attackSpeed)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Attack");
                //deal damage
                attackTimer = 0;
            }
        }      

        if (attackTimer < weapon.attackSpeed)
        {
            attackTimer += 1;
        }
    }
}

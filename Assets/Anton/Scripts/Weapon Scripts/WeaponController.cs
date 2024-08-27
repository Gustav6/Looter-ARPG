using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TreeEditor;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] public ScriptableObjectsWeapons weapon;
    public CircleCollider2D damageHitbox;

    private int timer;
    void Start()
    {
        timer = weapon.attackSpeed;
        damageHitbox.radius = weapon.range;
    }

    void Update()
    {
        if (timer >= weapon.attackSpeed)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Attack");
                //deal damage
                timer = 0;
            }
        }

        if (timer < weapon.attackSpeed)
        {
            timer += 1;
        }    
    }
}

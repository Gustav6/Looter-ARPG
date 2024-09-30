using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Weapon Object", menuName = "Inventory System/Items/Weapon")]
public class WeaponObject : ItemObject
{
    public float attack;
    public float defense;
    public void Awake()
    {
        type = ItemType.Weapons;
    }
}

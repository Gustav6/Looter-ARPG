using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Equipment Object", menuName = "Inventory System/Items/Equipment")]
public class Equipment : ItemObject
{
    public float attack;
    public float defense;
    public void Awake()
    {
        type = ItemType.Equipment;
    }
}

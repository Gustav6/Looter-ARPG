using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Weapon Object", menuName = "Inventory System/Items/Potion")]

public class Potions : ItemObject
{
    public int restoreHealth;
    public void Awake()
    {
        type = ItemType.Potions;
    }
}

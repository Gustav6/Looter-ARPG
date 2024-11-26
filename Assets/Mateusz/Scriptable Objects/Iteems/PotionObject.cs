using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Consumables Object", menuName = "Inventory System/Items/Consumables")]

public class Consumables : ItemObject
{
    public int restoreHealth;
    public void Awake()
    {
        type = ItemType.Consumables;
    }
}

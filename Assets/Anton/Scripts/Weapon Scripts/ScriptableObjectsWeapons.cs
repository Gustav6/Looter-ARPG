using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class ScriptableObjectsWeapons : ScriptableObject
{
    public new string name;
    public string description;

    public Sprite sprite;

    public int damage;
    public int attackSpeed;
    public int staminaCost;
    public int critChanse;
    public int range;
}

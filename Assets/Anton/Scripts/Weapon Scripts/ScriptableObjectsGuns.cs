using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
public class ScriptableObjectsGuns : ScriptableObject
{
    public new string name;
    public string description;

    public Sprite sprite;
    public GameObject bulletPrefab;

    [Header("Stats")]
    #region Stats 
    public int damage;  
    public int Ammo;
    public int critChanse;
    public float fireRate;
    public float fireForce;
    public float reloadTime;
    #endregion

    [Header("Effects")]
    #region Effects 
    public bool dubbelShot;
    public bool piercingShot;
    public int amountOfPircableEnemies;
    #endregion
}

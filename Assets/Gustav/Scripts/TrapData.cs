using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "TrapData", menuName = "TileData/Trap")]
public class TrapData : TileData
{
    public float damage;

    private OnCollisionEnter2D onCollisionEnter;
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopUpText : MonoBehaviour
{
    TextMeshPro textMeshPro;
    string text;
    void Start()
    {
        textMeshPro.text = WeaponController.Damage.ToString();
    } 
}
    
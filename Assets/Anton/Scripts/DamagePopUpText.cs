using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopUpText : MonoBehaviour
{
    public TextMeshPro textMeshPro;
    float timer;
    public float destructTimer;
    void Start()
    {
        timer = destructTimer;
        textMeshPro.SetText(GunController.Damage.ToString());
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
    
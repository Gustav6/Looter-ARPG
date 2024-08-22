using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        Vector2 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = rotation;
    }
}

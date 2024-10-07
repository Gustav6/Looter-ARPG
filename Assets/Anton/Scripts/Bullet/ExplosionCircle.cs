using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionCircle : MonoBehaviour
{
    float timer;
    void Update()
    {
        transform.localScale += new Vector3(2f, 2f);
        timer += Time.deltaTime;

        if (timer >= 0.3)
        {
            Destroy(gameObject);
        }
    }
}

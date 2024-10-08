using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionCircle : MonoBehaviour
{
    float timer;
    private void Start()
    {
        transform.localScale = new Vector3(1, 1);
    }
    void Update()
    {
        transform.localScale += new Vector3(2f, 2f);
        timer += Time.deltaTime;

        if (timer >= 0.2)
        {
            Destroy(gameObject);
        }
    }
}

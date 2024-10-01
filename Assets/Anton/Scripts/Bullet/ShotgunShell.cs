using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunShell : MonoBehaviour
{
    float destroyBulletTimer = 2;
    public GameObject pelletPrefab;
    Transform gunAngle;
    Transform startTransform;

    private void Start()
    {      
        gunAngle = GameObject.FindWithTag("Gun").GetComponent<Transform>();
        startTransform.position = gunAngle.transform.position;

        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos -= transform.position;

        for (int i = 0; i < 5; i++)
        {
            if (mouseWorldPos.y > transform.position.y)
            {
                float rndF = Random.Range(-0.4f, 0.3f);
                GameObject bullet1 = Instantiate(pelletPrefab, transform.position, Quaternion.identity);
                bullet1.GetComponent<Rigidbody2D>().AddForce((gunAngle.transform.right - new Vector3(rndF, 0, 0)) * GunController.fireForce, ForceMode2D.Impulse);
            }
            else if (mouseWorldPos.y <= transform.position.y)
            {
                float rndF = Random.Range(-0.4f, 0.3f);
                GameObject bullet1 = Instantiate(pelletPrefab, transform.position, Quaternion.identity);
                bullet1.GetComponent<Rigidbody2D>().AddForce((gunAngle.transform.right - new Vector3(0, rndF, 0)) * GunController.fireForce, ForceMode2D.Impulse);
            }            
        }
        gunAngle.transform.position = startTransform.position;
    }
    void Update()
    {   
        destroyBulletTimer -= Time.deltaTime;
        if (destroyBulletTimer <= 0)
        {
            Destroy(gameObject);
        }
    }
}

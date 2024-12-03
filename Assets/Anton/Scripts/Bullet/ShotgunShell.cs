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
        startTransform = GameObject.FindWithTag("Gun").GetComponent<Transform>();

        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos -= transform.position;

        for (int i = 0; i < 3; i++)
        {
            if (mouseWorldPos.y > transform.position.y)
            {
                for (int a = 0; a < 5; a++)
                {
                    GameObject bullet1 = Instantiate(pelletPrefab, transform.position, Quaternion.identity);
                    bullet1.GetComponent<Rigidbody2D>().AddForce((gunAngle.transform.right -= new Vector3(0.5f, 0, 0)) * GunController.Instance.fireForce, ForceMode2D.Impulse);
                }
                gunAngle.transform.position = startTransform.position;
                for (int t = 0; t < 5; t++)
                {
                    GameObject bullet2 = Instantiate(pelletPrefab, transform.position, Quaternion.identity);
                    bullet2.GetComponent<Rigidbody2D>().AddForce((gunAngle.transform.right += new Vector3(0.5f, 0, 0)) * GunController.Instance.fireForce, ForceMode2D.Impulse);
                }
            }
            else if (mouseWorldPos.y <= transform.position.y)
            {           
                GameObject bullet1 = Instantiate(pelletPrefab, transform.position, Quaternion.identity);
                bullet1.GetComponent<Rigidbody2D>().AddForce((gunAngle.transform.right - new Vector3(0, 0.5f, 0)) * GunController.Instance.fireForce, ForceMode2D.Impulse);
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

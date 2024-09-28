using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunShell : MonoBehaviour
{
    float destroyBulletTimer = 2;
    public GameObject pelletPrefab;
    Transform gunAngle;

    private void Start()
    {
        gunAngle = GameObject.FindWithTag("Gun").GetComponent<Transform>();

        GameObject bullet1 = Instantiate(pelletPrefab, transform.position, Quaternion.identity);
        bullet1.GetComponent<Rigidbody2D>().AddForce(gunAngle.transform.right * GunController.fireForce, ForceMode2D.Impulse);

        GameObject bullet2 = Instantiate(pelletPrefab, transform.position, Quaternion.identity);
        bullet2.GetComponent<Rigidbody2D>().AddForce((gunAngle.transform.right - new Vector3(0, 0.1f, 0)) * GunController.fireForce, ForceMode2D.Impulse);

        GameObject bullet3 = Instantiate(pelletPrefab, transform.position, Quaternion.identity);
        bullet3.GetComponent<Rigidbody2D>().AddForce((gunAngle.transform.right - new Vector3(0, -0.1f, 0)) * GunController.fireForce, ForceMode2D.Impulse);

        GameObject bullet4 = Instantiate(pelletPrefab, transform.position, Quaternion.identity);
        bullet4.GetComponent<Rigidbody2D>().AddForce((gunAngle.transform.right - new Vector3(0, 0.2f, 0)) * GunController.fireForce, ForceMode2D.Impulse);

        GameObject bullet5 = Instantiate(pelletPrefab, transform.position, Quaternion.identity);
        bullet5.GetComponent<Rigidbody2D>().AddForce((gunAngle.transform.right - new Vector3(0, -0.2f, 0)) * GunController.fireForce, ForceMode2D.Impulse);
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

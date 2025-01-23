using UnityEngine;

public class ChangeEquipedGun : MonoBehaviour
{
    public ScriptableObjectsGuns[] guns;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            int randomValue = Random.Range(0, 5);
            GunController.Instance.gun = guns[randomValue];
            GunController.Instance.sprite.sprite = guns[randomValue].sprite;
            GunController.Instance.gun.reloadTime = guns[randomValue].reloadTime;
            GunController.Instance.Damage = guns[randomValue].damage;
        }
    }
}
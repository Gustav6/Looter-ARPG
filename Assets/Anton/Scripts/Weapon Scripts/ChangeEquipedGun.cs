using UnityEngine;

public class ChangeEquipedGun : MonoBehaviour
{
    public ScriptableObjectsGuns[] guns;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            int randomValue = Random.Range(0, 4);
            GunController.Instance.gun = guns[randomValue];
            GunController.Instance.sprite.sprite = guns[randomValue].sprite;
        }
    }
}
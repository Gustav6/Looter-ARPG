using UnityEngine;

public class ChangeEquipedGun : MonoBehaviour
{
    public ScriptableObjectsGuns[] guns;
    void Update()
    {
        int randomValue = Random.Range(0, 4);

        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("ha");
            GunController.Instance.gun = guns[randomValue];
            GunController.Instance.sprite.sprite = guns[randomValue].sprite;
        }
    }
}

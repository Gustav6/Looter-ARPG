using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private void Update()
    {
        if (Player.Instance != null)
        {
            transform.position = Player.Instance.transform.position + new Vector3(0, 10, 0);
        }
    }
}

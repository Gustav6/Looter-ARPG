using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public Renderer ren;

    void Update()
    {
        ren.material.mainTextureOffset = new Vector2(Player.Instance.transform.position.x * .2f, Player.Instance.transform.position.y * .2f);
    }
}

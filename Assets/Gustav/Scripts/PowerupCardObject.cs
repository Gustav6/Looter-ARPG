using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PowerupCard", menuName = "Cards")]
public class PowerupCardObject : ScriptableObject
{
    public string description;
    public Sprite cardSprite;
}

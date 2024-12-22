using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerupCard : Selectable, IPointerClickHandler
{
    public GameObject descriptionObject;
    public TextMeshProUGUI descriptionText;
    public Image cardImage;

    public PowerupCardObject cardAttached;

    protected override void Start()
    {
        descriptionText.text = cardAttached.description;
        cardImage.sprite = cardAttached.cardSprite;

        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void RunWhenSelected()
    {
        base.RunWhenSelected();
    }

    public override void RunWhenDeselected()
    {
        base.RunWhenDeselected();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        // Add power up to stats
    }
}

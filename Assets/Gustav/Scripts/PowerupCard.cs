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

        TransitionSystem.AddTransition(new ColorTransition(cardImage, 0.15f, new Color(0.7f, 0.7f, 0.7f), TransitionType.SmoothStop2, RunAfterClick), cardImage.gameObject);
        TransitionSystem.AddTransition(new ScaleTransition(transform, 0.15f, new Vector2(1.05f, 1.05f), TransitionType.SmoothStop2), cardImage.gameObject);

        // Add power up to stats
    }

    private void RunAfterClick()
    {
        TransitionSystem.AddTransition(new ColorTransition(cardImage, 0.15f, Color.white, TransitionType.SmoothStop2), cardImage.gameObject);
        TransitionSystem.AddTransition(new ScaleTransition(transform, 0.15f, new Vector2(1.1f, 1.1f), TransitionType.SmoothStop2), cardImage.gameObject);
    }
}

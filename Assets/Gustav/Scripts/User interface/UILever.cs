using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILever : UIBaseScript, IPointerClickHandler
{
    [BoxGroup("Lever variables")]
    [SerializeField] private bool leverOn;

    [BoxGroup("Lever variables")]
    [SerializeField] private GameObject objectIndicator;

    private Image indicatorsImage;

    public override void Start()
    {
        indicatorsImage = objectIndicator.GetComponent<Image>();

        leverOn = true;
        UpdateIndicator(0);

        base.Start();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        leverOn = !leverOn;
        UpdateIndicator(0.1f);
    }

    public void UpdateIndicator(float timeItTakes)
    {
        if (leverOn)
        {
            if (timeItTakes <= 0)
            {
                objectIndicator.SetActive(true);
                return;
            }

            objectIndicator.SetActive(true);
            TransitionSystem.AddTransition(new ColorTransition(indicatorsImage, timeItTakes, Color.white, TransitionType.SmoothStop2), gameObject);
        }
        else
        {
            if (timeItTakes <= 0)
            {
                objectIndicator.SetActive(false);
                return;
            }

            TransitionSystem.AddTransition(new ColorTransition(indicatorsImage, timeItTakes, Color.white * 0, TransitionType.SmoothStop2, DisableIndicator), gameObject);
        }
    }

    private void DisableIndicator()
    {
        objectIndicator.SetActive(false);
    }
}

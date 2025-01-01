using UnityEngine;
using UnityEngine.EventSystems;

public class UIDropDownContent : UIBaseScript, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        TransitionSystem.AddTransition(new ColorTransition(transform, 0.1f, new Color(0.85f, 0.85f, 0.85f, 0.5f), TransitionType.SmoothStop2, RunAfterClickAnimation), gameObject);
    }

    private void RunAfterClickAnimation()
    {
        UIDropDown dropDown = GetComponentInParent<UIDropDown>();

        dropDown.ChangeResolution(transform);

        dropDown.HighlightResolution(transform);
    }
}

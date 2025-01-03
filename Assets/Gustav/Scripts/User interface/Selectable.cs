using NaughtyAttributes;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Selectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [field: SerializeField] public bool Selected { get; private set; }

    public event EventHandler<PointerEventData> PointerEnter, PointerExit;

    [Foldout("Enable Transitions")]
    [SerializeField] private UIMoveTransition[] moveTransitionsOnEnable;
    [Foldout("Enable Transitions")]
    [SerializeField] private UIScaleTransition[] scaleTransitionsOnEnable;
    [Foldout("Enable Transitions")]
    [SerializeField] private UIColorTransition[] colorTransitionsOnEnable;
    [Foldout("Enable Transitions")]
    [SerializeField] private UIRotationTransition[] rotationTransitionsOnEnable;

    [Foldout("Disable Transitions")]
    [SerializeField] private UIMoveTransition[] moveTransitionsOnDisable;
    [Foldout("Disable Transitions")]
    [SerializeField] private UIScaleTransition[] scaleTransitionsOnDisable;
    [Foldout("Disable Transitions")]
    [SerializeField] private UIColorTransition[] colorTransitionsOnDisable;
    [Foldout("Disable Transitions")]
    [SerializeField] private UIRotationTransition[] rotationTransitionsOnDisable;

    protected virtual void Start()
    {
        SetActiveStatus(false);
    }

    protected virtual void Update() { }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter?.Invoke(this, eventData);
        SetActiveStatus(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExit?.Invoke(this, eventData);
        SetActiveStatus(false);
    }

    public void SetActiveStatus(bool status)
    {
        Selected = status;

        if (Selected)
        {
            RunWhenSelected();
        }
        else
        {
            RunWhenDeselected();
        }
    }

    public virtual void RunWhenSelected()
    {
        List<UITransition[]> transitionList = new()
        {
            moveTransitionsOnEnable,
            scaleTransitionsOnEnable, 
            colorTransitionsOnEnable, 
            rotationTransitionsOnEnable,
        };

        TransitionSystem.AddUITransitions(transitionList);
    }

    public virtual void RunWhenDeselected()
    {
        List<UITransition[]> transitionList = new()
        {
            moveTransitionsOnDisable,
            scaleTransitionsOnDisable,
            colorTransitionsOnDisable,
            rotationTransitionsOnDisable,
        };

        TransitionSystem.AddUITransitions(transitionList);
    }
}
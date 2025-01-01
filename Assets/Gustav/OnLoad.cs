using NaughtyAttributes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnLoad : MonoBehaviour
{
    [Foldout("Enable Transitions")]
    [SerializeField] private UIMoveTransition[] moveTransitionsOnEnable;
    [Foldout("Enable Transitions")]
    [SerializeField] private UIScaleTransition[] scaleTransitionsOnEnable;
    [Foldout("Enable Transitions")]
    [SerializeField] private UIColorTransition[] colorTransitionsOnEnable;
    [Foldout("Enable Transitions")]
    [SerializeField] private UIRotationTransition[] rotationTransitionsOnEnable;
    [Foldout("Enable Transitions")]
    [SerializeField] private UIRectSizeTransition[] rectSizeTransitionsOnEnable;


    private void OnEnable()
    {
        List<UITransition[]> transitionList = new()
        {
            moveTransitionsOnEnable,
            scaleTransitionsOnEnable,
            colorTransitionsOnEnable,
            rotationTransitionsOnEnable,
            rectSizeTransitionsOnEnable,
        };

        TransitionSystem.AddUITransitions(transitionList);
    }
}

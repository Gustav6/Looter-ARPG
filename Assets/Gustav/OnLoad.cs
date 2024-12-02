using NaughtyAttributes;
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
    [SerializeField] private UIRectSizeTransition[] RectSizeTransitionsOnEnable;

    private void OnEnable()
    {
        foreach (UIMoveTransition transition in moveTransitionsOnEnable)
        {
            Transition temp;

            if (transition.targetTransform != null)
            {
                temp = new MoveTransition(transition.effected, transition.duration, transition.targetTransform.position + (Vector3)transition.target, transition.transitionType, transition.targetInWorldPosition);
            }
            else
            {
                temp = new MoveTransition(transition.effected, transition.duration, transition.target, transition.transitionType, transition.targetInWorldPosition);
            }

            TransitionSystem.AddTransition(temp, transition.effected.gameObject, transition.overrideExistingTransitions);
        }
        foreach (UIScaleTransition transition in scaleTransitionsOnEnable)
        {
            Transition temp;

            if (transition.targetTransform != null)
            {
                temp = new ScaleTransition(transition.effected, transition.duration, transition.targetTransform.localScale + (Vector3)transition.target, transition.transitionType);
            }
            else
            {
                temp = new ScaleTransition(transition.effected, transition.duration, transition.target, transition.transitionType);
            }

            TransitionSystem.AddTransition(temp, transition.effected.gameObject, transition.overrideExistingTransitions);
        }
        foreach (UIColorTransition transition in colorTransitionsOnEnable)
        {
            Transition temp;

            if (transition.effected.GetComponent<Image>() != null)
            {
                temp = new ColorTransition(transition.effected.GetComponent<Image>(), transition.duration, transition.target, transition.transitionType);
            }
            else if (transition.effected.GetComponent<TextMeshProUGUI>() != null)
            {
                temp = new ColorTransition(transition.effected.GetComponent<TextMeshProUGUI>(), transition.duration, transition.target, transition.transitionType);
            }
            else
            {
                continue;
            }

            TransitionSystem.AddTransition(temp, transition.effected.gameObject, transition.overrideExistingTransitions);
        }

        foreach (UIRectSizeTransition transition in RectSizeTransitionsOnEnable)
        {
            Transition temp = new RectSizeTransition(transition.effected, transition.duration, transition.target, transition.transitionType);

            TransitionSystem.AddTransition(temp, transition.effected.gameObject, transition.overrideExistingTransitions);
        }
    }
}

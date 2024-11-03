using NaughtyAttributes;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static System.TimeZoneInfo;

public abstract class UIBaseScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [field: SerializeField] public bool Active { get; private set; }

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

    public virtual void Start()
    {
        SetActiveStatus(false);
    }

    public virtual void Update()
    {
    }

    public void SetActiveStatus(bool activeStatus)
    {
        Active = activeStatus;

        if (activeStatus)
        {
            RunOnEnable();
        }
        else
        {
            RunOnDisable();
        }
    }

    public virtual void RunOnEnable()
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

            TransitionSystem.AddTransition(temp, transition.effected.gameObject); 
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

            TransitionSystem.AddTransition(temp, transition.effected.gameObject);
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

            TransitionSystem.AddTransition(temp, transition.effected.gameObject);
        }
        foreach (UIRotationTransition transition in rotationTransitionsOnEnable)
        {
            Transition temp;

            if (transition.targetTransform != null)
            {
                temp = new ScaleTransition(transition.effected, transition.duration, transition.targetTransform.rotation.eulerAngles + (Vector3)transition.target, transition.transitionType);
            }
            else
            {
                temp = new ScaleTransition(transition.effected, transition.duration, transition.target, transition.transitionType);
            }

            TransitionSystem.AddTransition(temp, transition.effected.gameObject);
        }
    }

    public virtual void RunOnDisable()
    {
        foreach (UIMoveTransition transition in moveTransitionsOnDisable)
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

            TransitionSystem.AddTransition(temp, transition.effected.gameObject);
        }
        foreach (UIScaleTransition transition in scaleTransitionsOnDisable)
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

            TransitionSystem.AddTransition(temp, transition.effected.gameObject);
        }
        foreach (UIColorTransition transition in colorTransitionsOnDisable)
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

            TransitionSystem.AddTransition(temp, transition.effected.gameObject);
        }
        foreach (UIRotationTransition transition in rotationTransitionsOnDisable)
        {
            Transition temp;

            if (transition.targetTransform != null)
            {
                temp = new ScaleTransition(transition.effected, transition.duration, transition.targetTransform.rotation.eulerAngles + (Vector3)transition.target, transition.transitionType);
            }
            else
            {
                temp = new ScaleTransition(transition.effected, transition.duration, transition.target, transition.transitionType);
            }

            TransitionSystem.AddTransition(temp, transition.effected.gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetActiveStatus(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetActiveStatus(false);
    }

    [Serializable]
    private struct UIScaleTransition
    {
        public TransitionType transitionType;

        public Transform effected;
        public float duration;

        public Vector2 target;
        public Transform targetTransform;
    }

    [Serializable]
    private struct UIMoveTransition
    {
        public TransitionType transitionType;

        public Transform effected;
        public float duration;

        public Vector2 target;
        public Transform targetTransform;
        public bool targetInWorldPosition;
    }

    [Serializable]
    private struct UIColorTransition
    {
        public TransitionType transitionType;

        public Transform effected;
        public float duration;

        public Color target;
    }

    [Serializable]
    private struct UIRotationTransition
    {
        public TransitionType transitionType;

        public Transform effected;
        public float duration;

        public Vector2 target;
        public Transform targetTransform;
    }
}

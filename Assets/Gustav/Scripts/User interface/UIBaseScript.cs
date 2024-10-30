using NaughtyAttributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UIBaseScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [field: SerializeField] public bool Active { get; private set; }

    [BoxGroup("Transitions")]
    [SerializeField] private UITransition[] runTransitionsOnEnable;
    [BoxGroup("Transitions")]
    [SerializeField] private UITransition[] runTransitionsOnDisable;

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
        Transition temp = null;

        foreach (UITransition transition in runTransitionsOnEnable)
        {
            if (transition.effected == null)
            {
                continue;
            }

            switch (transition.transitionVariant)
            {
                case TransitionVariant.color:
                    if (transition.effected.GetComponent<Image>() != null)
                    {
                        temp = new ColorTransition(transition.effected.GetComponent<Image>(), transition.timeItTakes, transition.newColor, transition.transitionType);
                    }
                    else if (transition.effected.GetComponent<TextMeshProUGUI>() != null)
                    {
                        temp = new ColorTransition(transition.effected.GetComponent<TextMeshProUGUI>(), transition.timeItTakes, transition.newColor, transition.transitionType);
                    }
                    break;
                case TransitionVariant.rotation:
                    temp = new RotationTransition(transition.effected.transform, transition.timeItTakes, transition.target, transition.transitionType);
                    break;
                case TransitionVariant.scale:
                    temp = new ScaleTransition(transition.effected.transform, transition.timeItTakes, transition.target, transition.transitionType);
                    break;
                case TransitionVariant.move:
                    temp = new MoveTransition(transition.effected.transform, transition.timeItTakes, transition.target, transition.transitionType);
                    break;
                default:
                    break;
            }

            TransitionSystem.AddTransition(temp, gameObject);
        }
    }

    public virtual void RunOnDisable()
    {
        Transition temp = null;

        foreach (UITransition transition in runTransitionsOnDisable)
        {
            if (transition.effected == null)
            {
                continue;
            }

            switch (transition.transitionVariant)
            {
                case TransitionVariant.color:
                    if (transition.effected.GetComponent<Image>() != null)
                    {
                        temp = new ColorTransition(transition.effected.GetComponent<Image>(), transition.timeItTakes, transition.newColor, transition.transitionType);
                    }
                    else if (transition.effected.GetComponent<TextMeshProUGUI>() != null)
                    {
                        temp = new ColorTransition(transition.effected.GetComponent<TextMeshProUGUI>(), transition.timeItTakes, transition.newColor, transition.transitionType);
                    }
                    else
                    {
                        continue;
                    }
                    break;
                case TransitionVariant.rotation:
                    temp = new RotationTransition(transition.effected.transform, transition.timeItTakes, transition.target, transition.transitionType);
                    break;
                case TransitionVariant.scale:
                    temp = new ScaleTransition(transition.effected.transform, transition.timeItTakes, transition.target, transition.transitionType);
                    break;
                case TransitionVariant.move:
                    temp = new MoveTransition(transition.effected.transform, transition.timeItTakes, transition.target, transition.transitionType);
                    break;
                default:
                    break;
            }

            TransitionSystem.AddTransition(temp, gameObject);
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
    private struct UITransition
    {
        public TransitionVariant transitionVariant;
        public TransitionType transitionType;

        public GameObject effected;
        public float timeItTakes;

        public Vector2 target;
        public Color newColor;
    }
}

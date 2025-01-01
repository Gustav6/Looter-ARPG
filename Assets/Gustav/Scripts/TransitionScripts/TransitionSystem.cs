using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.TimeZoneInfo;

public class TransitionSystem
{
    public static readonly Dictionary<GameObject, List<Transition>> transitionPairs = new();

    public static void Update()
    {
        foreach (GameObject id in transitionPairs.Keys)
        {
            foreach (Transition transition in transitionPairs[id])
            {
                transition.Update();
            }
        }

        for (int i = transitionPairs.Keys.Count - 1; i >= 0; i--)
        {
            for (int j = transitionPairs.ElementAt(i).Value.Count - 1; j >= 0; j--)
            {
                Transition currentTransition = transitionPairs.ElementAt(i).Value[j];

                if (currentTransition.isRemoved)
                {
                    currentTransition.execute?.Invoke();
                    transitionPairs.ElementAt(i).Value.Remove(currentTransition);

                    if (transitionPairs.ElementAt(i).Value.Count == 0)
                    {
                        transitionPairs.Remove(transitionPairs.ElementAt(i).Key);
                    }
                }
            }
        }
    }

    public static void AddTransition(Transition transition, GameObject id, bool removeTransitionsWithSameID = true, bool loop = false)
    {
        transition.loop = loop;

        if (transitionPairs.ContainsKey(id))
        {
            if (removeTransitionsWithSameID)
            {
                RemoveTransitionsWithSameID(id, transition);
            }

            transitionPairs[id].Add(transition);
        }
        else
        {
            transitionPairs.Add(id, new List<Transition>() { transition } );
        }

        transition.Start();
    }

    public static void RemoveTransitionsWithSameID(GameObject id, Transition transition)
    {
        for (int i = transitionPairs[id].Count - 1; i >= 0; i--)
        {
            if (transition.GetType() == transitionPairs[id].ElementAt(i).GetType())
            {
                transitionPairs[id].RemoveAt(i);
            }
        }
    }

    public static void ClearTransitionList()
    {
        transitionPairs.Clear();
    }

    public static float Crossfade(float t, float a, float b) => ((1 - t) * a) + (t * b);

    public static float Flip(float t) => 1 - t;

    public static float BounceClampTop(float t) => Mathf.Abs(t);
    public static float BounceClampBottom(float t) => 1 - Mathf.Abs(1 - t);

    public static float Scale(float t) => t * t;
    public static float ReverseScale(float t) => t * (1 - t);

    public static float SmoothStart2(float t) => t * t;
    public static float SmoothStart3(float t) => t * t * t;
    public static float SmoothStart4(float t) => t * t * t * t;

    public static float SmoothStop2(float t) => 1 - ((1 - t) * (1 - t));
    public static float SmoothStop3(float t) => 1 - ((1 - t) * (1 - t) * (1 - t));
    public static float SmoothStop4(float t) => 1 - ((1 - t) * (1 - t) * (1 - t) * (1 - t));

    public static float SinCurve(float t, float interval, float amplitude, float offset = 0) => (amplitude * math.sin(t * math.PI * interval)) + offset;
    public static float CosCurve(float t, float interval, float amplitude, float offset = 0) => (amplitude * math.cos(t * math.PI * interval)) + offset;

    public static float NormalizedBezier3(float t, float windUp, float overShoot)
    {
        float s = 1 - t;
        float t2 = t * t;
        float s2 = s * s;
        float t3 = t2 * t;
        return (3 * windUp * s2 * t) + (3 * overShoot * s * t2) + t3;
    }
    public static float NormalizedBezier4(float t, float b, float c, float d)
    {
        float s = 1 - t;
        float t2 = t * t;
        float s2 = s * s;
        float t3 = t2 * t;
        float s3 = s2 * s;
        float t4 = t3 * t;
        return (4 * b * s3 * t) + (8 * c * s2 * t2) + (4 * d * s * t3) + t4;
    }

    public static void AddUITransitions(List<UITransition[]> transitionList)
    {
        Transition temp = null;

        foreach (UITransition[] transitionArray in transitionList)
        {
            foreach (UITransition transition in transitionArray)
            {
                if (transition.effected == null)
                {
                    continue;
                }

                if (transition is UIScaleTransition scaleT)
                {
                    if (transition.transitionType == TransitionType.SinCurve || transition.transitionType == TransitionType.CosCurve)
                    {
                        temp = new ScaleTransition(scaleT.effected, scaleT.duration, scaleT.transitionType, scaleT.interval, scaleT.amplitude, scaleT.offset);
                    }
                    else
                    {
                        if (scaleT.targetTransform != null)
                        {
                            temp = new ScaleTransition(scaleT.effected, scaleT.duration, scaleT.targetTransform.localScale + (Vector3)scaleT.target, scaleT.transitionType);
                        }
                        else
                        {
                            temp = new ScaleTransition(scaleT.effected, scaleT.duration, scaleT.target, scaleT.transitionType);
                        }
                    }
                }
                else if (transition is UIMoveTransition moveT)
                {
                    if (transition.transitionType == TransitionType.SinCurve || transition.transitionType == TransitionType.CosCurve)
                    {
                        temp = new MoveTransition(moveT.effected, moveT.duration, moveT.transitionType, moveT.interval, moveT.amplitude, moveT.offset, moveT.targetInWorld);
                    }
                    else
                    {
                        if (moveT.targetTransform != null)
                        {
                            temp = new MoveTransition(moveT.effected, moveT.duration, moveT.targetTransform.position + (Vector3)moveT.target, moveT.transitionType, moveT.targetInWorld);
                        }
                        else
                        {
                            temp = new MoveTransition(moveT.effected, moveT.duration, moveT.target, moveT.transitionType, moveT.targetInWorld);
                        }
                    }
                }
                else if (transition is UIRectSizeTransition rectSizeT)
                {
                    if (transition.transitionType == TransitionType.SinCurve || transition.transitionType == TransitionType.CosCurve)
                    {
                        temp = new RectSizeTransition((RectTransform)rectSizeT.effected, rectSizeT.duration, rectSizeT.transitionType, rectSizeT.interval, rectSizeT.amplitude, rectSizeT.offset);
                    }
                    else
                    {
                        temp = new RectSizeTransition((RectTransform)rectSizeT.effected, rectSizeT.duration, rectSizeT.target, rectSizeT.transitionType);
                    }
                }
                else if (transition is UIRotationTransition rotationT)
                {
                    if (transition.transitionType == TransitionType.SinCurve || transition.transitionType == TransitionType.CosCurve)
                    {
                        temp = new RotationTransition(rotationT.effected, rotationT.duration, rotationT.transitionType, rotationT.interval, rotationT.amplitude, rotationT.offset);
                    }
                    else
                    {
                        if (rotationT.targetTransform != null)
                        {
                            temp = new RotationTransition(rotationT.effected, rotationT.duration, rotationT.targetTransform.rotation.eulerAngles + rotationT.target, rotationT.transitionType);
                        }
                        else
                        {
                            temp = new RotationTransition(rotationT.effected, rotationT.duration, rotationT.target, rotationT.transitionType);
                        }
                    }
                }
                else if (transition is UIColorTransition colorT)
                {
                    if (colorT.effected.GetComponent<Image>() != null)
                    {
                        temp = new ColorTransition(colorT.effected.GetComponent<Image>(), colorT.duration, colorT.target, colorT.transitionType);
                    }
                    else if (colorT.effected.GetComponent<TextMeshProUGUI>() != null)
                    {
                        temp = new ColorTransition(colorT.effected.GetComponent<TextMeshProUGUI>(), colorT.duration, colorT.target, colorT.transitionType);
                    }
                }

                if (temp == null)
                {
                    continue;
                }

                AddTransition(temp, transition.effected.gameObject, transition.overrideExistingTransitions, transition.loopAnimation);
            }
        }
    }
}

public abstract class UITransition
{
    public Transform effected;

    public TransitionType transitionType;

    public float duration;

    public bool overrideExistingTransitions;
    public bool loopAnimation;
}

[Serializable]
public class UIScaleTransition : UITransition
{
    public Vector2 target;
    public Transform targetTransform;

    public float interval;
    public Vector2 offset, amplitude;
}

[Serializable]
public class UIMoveTransition : UITransition
{
    public Vector2 target;
    public Transform targetTransform;
    public bool targetInWorld;

    public float interval;
    public Vector2 offset, amplitude;
}

[Serializable]
public class UIRectSizeTransition : UITransition
{
    public Vector2 target;

    public float interval;
    public Vector2 offset, amplitude;
}

[Serializable]
public class UIColorTransition : UITransition
{
    public Color target;
}

[Serializable]
public class UIRotationTransition : UITransition
{
    public Vector3 target;
    public Transform targetTransform;

    public float interval;
    public Vector2 offset, amplitude;
}

public enum TransitionType
{
    SmoothStart2,
    SmoothStart3,
    SmoothStart4,

    SmoothStop2,
    SmoothStop3,
    SmoothStop4,

    SinCurve,
    CosCurve,
}

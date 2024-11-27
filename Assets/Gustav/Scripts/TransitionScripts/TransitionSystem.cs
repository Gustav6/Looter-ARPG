using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

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

    public static float SinCurve(float t, float interval, float amplitude, float offset = 0) => (amplitude * math.sin(t * math.PI * 0.5f * interval)) + offset;
    public static float CosCurve(float t, float interval, float amplitude, float offset = 0) => (amplitude * math.cos(t * math.PI * 0.5f * interval)) + offset;

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

    internal static float SinCurve(float v, float curveInterval, float curveAmplitude, Vector2 curveOffset)
    {
        throw new NotImplementedException();
    }
}

public enum TransitionType
{
    SmoothStart2,
    SmoothStart3,
    SmoothStart4,

    SmoothStop2,
    SmoothStop3,
    SmoothStop4,
}

public enum CurveType
{
    SinCurve,
    CosCurve,
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class TransitionSystem
{
    private static readonly Dictionary<GameObject, List<Transition>> transitionPairs = new();

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
                if (transitionPairs.ElementAt(i).Value.ElementAt(j).isRemoved)
                {
                    transitionPairs.ElementAt(i).Value.ElementAt(j).execute?.Invoke();
                    transitionPairs.ElementAt(i).Value.RemoveAt(j);

                    if (transitionPairs.ElementAt(i).Value.Count == 0)
                    {
                        transitionPairs.Remove(transitionPairs.ElementAt(i).Key);
                    }
                }
            }
        }
    }

    public static void ClearTransitionList()
    {
        transitionPairs.Clear();
    }

    public static void AddTransition(Transition transition, GameObject id, bool removeTransitionsWithSameID = true, bool loop = false)
    {
        transition.loop = loop;

        if (transitionPairs.ContainsKey(id))
        {
            if (removeTransitionsWithSameID)
            {
                for (int i = transitionPairs[id].Count - 1; i >= 0; i--)
                {
                    if (transition.GetType() == transitionPairs[id].ElementAt(i).GetType())
                    {
                        transitionPairs[id].RemoveAt(i);
                    }
                }
            }

            transitionPairs[id].Add(transition);
        }
        else
        {
            transitionPairs.Add(id, new List<Transition>() { transition });
        }

        transition.Start();
    }

    public static float BounceClampTop(float t)
    {
        return Mathf.Abs(t);
    }
    public static float BounceClampBottom(float t)
    {
        return 1 - Mathf.Abs(1 - t);
    }

    public static float Flip(float t)
    {
        return 1 - t;
    }

    public static float Scale(float t)
    {
        return t * t;
    }
    public static float ReverseScale(float t)
    {
        return t * (1 - t);
    }

    public static float SmoothStart2(float t)
    {
        return t * t;
    }

    public static float SmoothStart3(float t)
    {
        return t * t * t;
    }

    public static float SmoothStart4(float t)
    {
        return t * t * t * t;
    }

    public static float SmoothStop2(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }

    public static float SmoothStop3(float t)
    {
        return 1 - (1 - t) * (1 - t) * (1 - t);
    }

    public static float SmoothStop4(float t)
    {
        return 1 - (1 - t) * (1 - t) * (1 - t) * (1 - t);
    }

    public static float SinCurve(float repetitions, float amplitude, float t)
    {
        return math.sin(t * math.PI * repetitions) * amplitude * -1;
    }

    public static float NormalizedBezier3(float windUp, float overShoot, float t)
    {
        float s = 1 - t;
        float t2 = t * t;
        float s2 = s * s;
        float t3 = t2 * t;
        return (3 * windUp * s2 * t) + (3 * overShoot * s * t2) + (t3);
    }
    public static float NormalizedBezier4(float b, float c, float d, float t)
    {
        float s = 1 - t;
        float t2 = t * t;
        float s2 = s * s;
        float t3 = t2 * t;
        float s3 = s2 * s;
        float t4 = t3 * t;
        return (4 * b * s3 * t) + (8 * c * s2 * t2) + (4 * d * s * t3) + (t4);
    }

    public static float Crossfade(float a, float b, float t)
    {
        return (1 - t) * a + t * b;
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

    SinCurve,
    CosCurve,
}

public enum TransitionVariant
{
    color,
    rotation,
    scale,
    move
}

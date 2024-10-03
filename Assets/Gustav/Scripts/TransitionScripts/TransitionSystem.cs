using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class TransitionSystem
{
    public static List<Transition> transitions = new();

    public static void Update()
    {
        for (int i = 0; i < transitions.Count; i++)
        {
            transitions[i].Update();
        }

        for (int i = transitions.Count - 1; i >= 0; i--)
        {
            if (transitions[i].isRemoved)
            {
                transitions[i].executeOnCompletion?.Invoke();
                transitions.RemoveAt(i);
            }
        }
    }

    //public static void AddMoveTransition(MoveTransition moveTransition)
    //{
    //    transitions.Add(moveTransition);
    //    moveTransition.Start();
    //}
    //public static void AddScaleTransition(ScaleTransition scaleTransition)
    //{
    //    transitions.Add(scaleTransition);
    //    scaleTransition.Start();
    //}
    //public static void AddColorTransition(ColorTransition colorTransition)
    //{
    //    transitions.Add(colorTransition);
    //    colorTransition.Start();
    //}
    //public static void AddRotationTransition(RotationTransition rotationTransition)
    //{
    //    transitions.Add(rotationTransition);
    //    rotationTransition.Start();
    //}

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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transition
{
    public delegate void ExecuteAfterTransition();
    public ExecuteAfterTransition execute;

    protected TransitionType? transitionType;
    protected CurveType? curveType;

    private protected float curveInterval, curveAmplitude;
    private protected Vector2 curveOffset;

    public bool loop;

    public bool isRemoved;

    public float timer;
    public float timerMax;

    protected float t;

    public virtual void Start()
    {
        if (timerMax <= 0)
        {
            OnInstantTransition();
        }

        execute += RunAfterTransition;
    }

    public virtual void Update()
    {
        if (Time.timeScale > 0)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer += Time.unscaledDeltaTime;
        }

        if (!loop && timer >= timerMax)
        {
            isRemoved = true;
        }

        if (transitionType != null)
        {
            t = transitionType switch
            {
                TransitionType.SmoothStart2 => TransitionSystem.SmoothStart2(timer / timerMax),
                TransitionType.SmoothStart3 => TransitionSystem.SmoothStart3(timer / timerMax),
                TransitionType.SmoothStart4 => TransitionSystem.SmoothStart4(timer / timerMax),
                TransitionType.SmoothStop2 => TransitionSystem.SmoothStop2(timer / timerMax),
                TransitionType.SmoothStop3 => TransitionSystem.SmoothStop3(timer / timerMax),
                TransitionType.SmoothStop4 => TransitionSystem.SmoothStop4(timer / timerMax),
                _ => 0,
            };
        }
        else if (curveType != null)
        {
            t = curveType switch
            {
                CurveType.SinCurve => TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude),
                CurveType.CosCurve => TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude),
                _ => 0,
            };
        }
    }

    public abstract void OnInstantTransition();
    public abstract void RunAfterTransition();
}

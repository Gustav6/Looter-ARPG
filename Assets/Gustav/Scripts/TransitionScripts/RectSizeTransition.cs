using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class RectSizeTransition : Transition
{
    private readonly RectTransform rectTransform;

    private readonly Vector2 startingSize;
    private readonly Vector2 targetSize;

    public RectSizeTransition(RectTransform rt, float time, Vector2 target, TransitionType type, ExecuteAfterTransition execute = null)
    {
        rectTransform = rt;
        
        timerMax = time;
        transitionType = type;

        startingSize = rt.sizeDelta;
        targetSize = target;

        this.execute += execute;
    }

    public RectSizeTransition(RectTransform rt, float time, TransitionType type, float interval, Vector2 amplitude, Vector2 offset, ExecuteAfterTransition execute = null)
    {
        rectTransform = rt;

        timerMax = time;
        transitionType = type;

        curveInterval = interval;
        curveAmplitude = amplitude;
        curveOffset = offset;

        this.execute += execute;
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();

        if (rectTransform == null)
        {
            isRemoved = true;
            return;
        }

        rectTransform.sizeDelta = transitionType switch
        {
            TransitionType.SinCurve => (Vector3)new Vector2(TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude.x) + curveOffset.x, TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude.y) + curveOffset.y),
            TransitionType.CosCurve => (Vector3)new Vector2(TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude.x) + curveOffset.x, TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude.y) + curveOffset.y),
            _ => Vector2.Lerp(startingSize, targetSize, t),
        };
    }
    public override void RunAfterTransition()
    {
        SetSizeToTarget();
    }

    public override void OnInstantTransition()
    {
        SetSizeToTarget();

        isRemoved = true;
    }

    private void SetSizeToTarget()
    {
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = targetSize;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public RectSizeTransition(RectTransform rt, float time, CurveType type, float interval, float amplitude, Vector2 offset, ExecuteAfterTransition execute = null)
    {
        rectTransform = rt;

        timerMax = time;
        curveType = type;

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

        if (transitionType != null)
        {
            rectTransform.sizeDelta = Vector2.Lerp(startingSize, targetSize, t);
        }
        else if (curveType != null)
        {
            rectTransform.sizeDelta = new Vector2(t + curveOffset.x, t + curveOffset.y);
        }
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

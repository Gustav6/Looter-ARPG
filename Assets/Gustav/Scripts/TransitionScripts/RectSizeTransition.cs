using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectSizeTransition : Transition
{
    private RectTransform rectTransform;

    private Vector2 startingSize;
    private Vector2 targetSize;

    private float t;

    public RectSizeTransition(RectTransform rt, float time, Vector2 target, TransitionType transition, ExecuteAfterTransition execute = null)
    {
        rectTransform = rt;
        
        timerMax = time;
        this.transition = transition;

        startingSize = rt.sizeDelta;
        targetSize = target;

        this.execute += execute;
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        if (rectTransform == null)
        {
            isRemoved = true;
            return;
        }

        t = transition switch
        {
            TransitionType.SmoothStart2 => TransitionSystem.SmoothStart2(timer / timerMax),
            TransitionType.SmoothStart3 => TransitionSystem.SmoothStart3(timer / timerMax),
            TransitionType.SmoothStart4 => TransitionSystem.SmoothStart4(timer / timerMax),
            TransitionType.SmoothStop2 => TransitionSystem.SmoothStop2(timer / timerMax),
            TransitionType.SmoothStop3 => TransitionSystem.SmoothStop3(timer / timerMax),
            TransitionType.SmoothStop4 => TransitionSystem.SmoothStop4(timer / timerMax),
            _ => 0,
        };

        rectTransform.sizeDelta = Vector2.Lerp(startingSize, targetSize, t);

        base.Update();
    }
    public override void RunAfterTransition()
    {
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = targetSize;
        }
        else
        {
            isRemoved = true;
            return;
        }

        base.RunAfterTransition();
    }
}

using UnityEngine;

public class ScaleTransition : Transition
{
    private Transform transform;

    private Vector3 startingScale;
    private Vector3 targetScale;

    private float t;

    public ScaleTransition(Transform t, float time, Vector3 target, TransitionType transition, ExecuteAfterTransition execute = null)
    {
        transform = t;

        timerMax = time;
        this.transition = transition;

        startingScale = t.localScale;
        targetScale = target;

        this.execute += execute;
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        if (transform == null)
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

        transform.localScale = Vector3.Lerp(startingScale, targetScale, t);

        base.Update();
    }

    public override void RunAfterTransition()
    {
        if (transform != null)
        {
            transform.localScale = targetScale;
        }
        else
        {
            isRemoved = true;
            return;
        }

        transform.localScale = targetScale;

        base.RunAfterTransition();
    }
}

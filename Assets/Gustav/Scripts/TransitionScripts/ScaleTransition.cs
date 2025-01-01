using UnityEngine;

public class ScaleTransition : Transition
{
    private readonly Transform transform;

    private readonly Vector3 startingScale;
    private readonly Vector3 targetScale;

    public ScaleTransition(Transform t, float time, Vector3 target, TransitionType type, ExecuteAfterTransition execute = null)
    {
        transform = t;

        timerMax = time;
        transitionType = type;

        startingScale = t.localScale;
        targetScale = target;

        this.execute += execute;
    }

    public ScaleTransition(Transform t, float time, TransitionType type, float interval, Vector2 amplitude, Vector2 offset, ExecuteAfterTransition execute = null)
    {
        transform = t;

        timerMax = time;
        transitionType = type;

        curveInterval = interval;
        curveAmplitude = amplitude;
        curveOffset = offset;

        startingScale = t.localScale;

        this.execute += execute;
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();

        if (transform == null)
        {
            isRemoved = true;
            return;
        }

        transform.localScale = transitionType switch
        {
            TransitionType.SinCurve => (Vector3)new Vector2(TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude.x) + curveOffset.x, TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude.y) + curveOffset.y),
            TransitionType.CosCurve => (Vector3)new Vector2(TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude.x) + curveOffset.x, TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude.y) + curveOffset.y),
            _ => Vector3.Lerp(startingScale, targetScale, t),
        };
    }

    public override void RunAfterTransition()
    {
        SetScaleToTarget();
    }

    public override void OnInstantTransition()
    {
        SetScaleToTarget();

        isRemoved = true;
    }

    private void SetScaleToTarget()
    {
        if (transform != null)
        {
            if (transitionType == TransitionType.SinCurve || transitionType == TransitionType.CosCurve)
            {
                transform.localScale = curveOffset;
            }
            else
            {
                transform.localScale = targetScale;
            }
        }
    }
}

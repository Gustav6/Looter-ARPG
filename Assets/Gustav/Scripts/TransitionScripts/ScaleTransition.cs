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

    public ScaleTransition(Transform t, float time, CurveType type, float interval, float amplitude, Vector2 offset, ExecuteAfterTransition execute = null)
    {
        transform = t;

        timerMax = time;
        curveType = type;

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

        if (transitionType != null)
        {
            transform.localScale = Vector3.Lerp(startingScale, targetScale, t);
        }
        else if (curveType != null)
        {
            transform.localScale = new Vector2(t + curveOffset.x, t + curveOffset.y);
        }
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
            if (transitionType != null)
            {
                transform.localScale = targetScale;
            }
            else if (curveType != null)
            {
                transform.localScale = curveOffset;
            }
        }
    }
}

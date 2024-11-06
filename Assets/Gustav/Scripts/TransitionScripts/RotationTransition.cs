using UnityEngine;

public class RotationTransition : Transition
{
    private readonly Transform transform;

    private readonly Vector3 startingRotation;
    private readonly Vector3 targetRotation;

    public RotationTransition(Transform t, float time, Vector3 target, TransitionType type, ExecuteAfterTransition execute = null)
    {
        transform = t;

        timerMax = time;
        transitionType = type;

        startingRotation = t.rotation.eulerAngles;
        targetRotation = target;

        this.execute += execute;
    }

    public RotationTransition(Transform t, float time, CurveType type, float interval, float amplitude, Vector2 offset, ExecuteAfterTransition execute = null)
    {
        transform = t;

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

        if (transform == null)
        {
            isRemoved = true;
            return;
        }

        if (transitionType != null)
        {
            transform.Rotate(Vector3.Lerp(startingRotation, targetRotation, t));
        }
        else if (curveType != null)
        {
            transform.Rotate(new Vector2(t + curveOffset.x, t + curveOffset.y));
        }
    }

    public override void RunAfterTransition()
    {
        SetRotationToTarget();
    }

    public override void OnInstantTransition()
    {
        SetRotationToTarget();

        isRemoved = true;
    }

    private void SetRotationToTarget()
    {
        if (transform != null)
        {
            transform.Rotate(targetRotation);
        }
    }
}

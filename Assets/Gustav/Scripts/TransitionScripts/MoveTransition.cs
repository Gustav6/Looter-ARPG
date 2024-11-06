using UnityEngine;

public class MoveTransition : Transition
{
    private readonly Transform transform;

    private readonly Vector3 startingPosition;
    private readonly Vector3 targetPosition;

    private readonly bool targetIsInWorldPosition;

    public MoveTransition(Transform t, float time, Vector3 target, TransitionType type, bool targetInWorld = true, ExecuteAfterTransition execute = null)
    {
        transform = t;

        timerMax = time;
        transitionType = type;

        startingPosition = t.position;
        targetPosition = target;
        targetIsInWorldPosition = targetInWorld;

        this.execute += execute;
    }

    public MoveTransition(Transform t, float time, CurveType type, float interval, float amplitude, Vector2 offset, bool targetInWorld = true, ExecuteAfterTransition execute = null)
    {
        transform = t;

        timerMax = time;
        curveType = type;

        startingPosition = t.position;
        targetIsInWorldPosition = targetInWorld;

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
            if (targetIsInWorldPosition)
            {
                transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
            }
            else
            {
                transform.position = Vector3.Lerp(startingPosition, startingPosition + targetPosition, t);
            }
        }
        else if (curveType != null)
        {
            transform.position = new Vector2(t + curveOffset.x, t + curveOffset.y);
        }
    }

    public override void RunAfterTransition()
    {
        SetPositionToTarget();
    }

    public override void OnInstantTransition()
    {
        SetPositionToTarget();

        isRemoved = true;
    }

    private void SetPositionToTarget()
    {
        if (transform != null)
        {
            if (targetIsInWorldPosition)
            {
                transform.position = targetPosition;
            }
            else
            {
                transform.position = startingPosition + targetPosition;
            }
        }
    }
}

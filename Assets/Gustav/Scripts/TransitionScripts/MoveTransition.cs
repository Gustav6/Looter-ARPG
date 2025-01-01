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

    public MoveTransition(Transform t, float time, TransitionType type, float interval, Vector2 amplitude, Vector2 offset, bool targetInWorld = true, ExecuteAfterTransition execute = null)
    {
        transform = t;

        timerMax = time;
        transitionType = type;

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

        switch (transitionType)
        {
            case TransitionType.SinCurve:

                if (targetIsInWorldPosition)
                {
                    transform.position = new Vector2(TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude.x) + curveOffset.x, TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude.y) + curveOffset.y);
                }
                else
                {
                    transform.localPosition = new Vector2(TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude.x) + curveOffset.x, TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude.y) + curveOffset.y);
                }
                break;
            case TransitionType.CosCurve:
                if (targetIsInWorldPosition)
                {
                    transform.position = new Vector2(TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude.x) + curveOffset.x, TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude.y) + curveOffset.y);
                }
                else
                {
                    transform.localPosition = new Vector2(TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude.x) + curveOffset.x, TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude.y) + curveOffset.y);
                }
                break;
            default:
                if (targetIsInWorldPosition)
                {
                    transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
                }
                else
                {
                    transform.position = Vector3.Lerp(startingPosition, startingPosition + targetPosition, t);
                }
                break;
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

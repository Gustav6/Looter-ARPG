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

        Debug.Log("Start: " + startingRotation);
        Debug.Log("Target: " + targetRotation);

        this.execute += execute;
    }

    public RotationTransition(Transform t, float time, TransitionType type, float interval, Vector2 amplitude, Vector2 offset, ExecuteAfterTransition execute = null)
    {
        transform = t;

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

        if (transform == null)
        {
            isRemoved = true;
            return;
        }

        switch (transitionType)
        {
            case TransitionType.SinCurve:
                transform.Rotate(new Vector2(TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude.x) + curveOffset.x, TransitionSystem.SinCurve(timer / timerMax, curveInterval, curveAmplitude.y) + curveOffset.y));
                break;
            case TransitionType.CosCurve:
                transform.Rotate(new Vector2(TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude.x) + curveOffset.x, TransitionSystem.CosCurve(timer / timerMax, curveInterval, curveAmplitude.y) + curveOffset.y));
                break;
            default:

                transform.localRotation = Quaternion.Euler(Vector3.Lerp(startingRotation, targetRotation, t));
                //transform.Rotate(Vector3.Lerp(startingRotation, targetRotation, t));
                break;
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
            transform.localRotation = Quaternion.Euler(targetRotation);
        }
    }
}

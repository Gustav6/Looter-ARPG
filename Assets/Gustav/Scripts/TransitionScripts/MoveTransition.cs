using UnityEngine;

public class MoveTransition : Transition
{
    private Transform transform;

    private Vector3 startingPosition;
    private Vector3 targetPosition;

    private readonly bool targetIsInWorldPosition;

    private float t;

    public MoveTransition(Transform t, float time, Vector3 target, TransitionType transition, bool targetInWorld = true, ExecuteAfterTransition execute = null)
    {
        transform = t;

        timerMax = time;
        this.transition = transition;

        startingPosition = t.position;
        targetPosition = target;
        targetIsInWorldPosition = targetInWorld;

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

        if (targetIsInWorldPosition)
        {
            transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
        }
        else
        {
            transform.position = Vector3.Lerp(startingPosition, startingPosition + targetPosition, t);
        }

        base.Update();
    }

    public override void RunAfterTransition()
    {
        if (transform)
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
        else
        {
            isRemoved = true;
            return;
        }

        base.RunAfterTransition();
    }
}

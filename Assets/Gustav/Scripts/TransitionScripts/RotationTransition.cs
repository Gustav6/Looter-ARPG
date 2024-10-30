using UnityEngine;

public class RotationTransition : Transition
{
    private Transform transform;

    private Vector3 startingRotation;
    private Vector3 targetRotation;

    private float t;

    public RotationTransition(Transform t, float time, Vector3 target, TransitionType transition, ExecuteAfterTransition execute = null)
    {
        transform = t;

        timerMax = time;
        this.transition = transition;

        startingRotation = t.rotation.eulerAngles;
        targetRotation = target;

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

        transform.Rotate(Vector3.Lerp(startingRotation, targetRotation, t));

        base.Update();
    }

    public override void RunAfterTransition()
    {
        transform.Rotate(targetRotation);

        base.RunAfterTransition();
    }
}

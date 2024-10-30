using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transition
{
    public delegate void ExecuteAfterTransition();
    public ExecuteAfterTransition execute;

    protected TransitionType transition;

    public bool loop;

    public bool isRemoved;

    public float timer;
    public float timerMax;

    public virtual void Start()
    {
        execute += RunAfterTransition;
    }

    public virtual void Update()
    {
        if (!loop)
        {
            if (Time.timeScale > 0)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
            }

            if (timer > timerMax)
            {
                isRemoved = true;
            }
        }
    }

    public virtual void RunAfterTransition()
    {

    }
}

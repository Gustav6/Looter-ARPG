using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    public bool isComplete { get; protected set; }

    protected float startTime;

    public float time => Time.time - startTime;

    protected Core core;

    protected Rigidbody2D body => core.body;

    protected Animator animator => core.animator;

    public StateMachine machine;

    public StateMachine parent;

    public State state => machine.state;

    protected void Set(State newState, bool forceReset = false)
    {
        machine.Set(newState, forceReset);
    }
    public void SetCore(Core _core)
    {
        machine = new StateMachine();
        core = _core;
    }

    public virtual void Enter(){ }
    public virtual void Do(){ }
    public virtual void DoFixed() { }
    
    public virtual void Exit() { }

    public void DoBranch()
    {
        Do();
        state?.DoBranch();
    }

    public void DoFixedBranch()
    {
        DoFixed();
        state?.DoFixedBranch();
    }

    public void Initialise(StateMachine _parent)
    {
        parent = _parent;
        isComplete = false;
        startTime = Time.time;
    }
}

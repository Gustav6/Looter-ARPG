using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    public EnemyProperties enemyProperties;
    public bool isComplete { get; protected set; }

    protected float startTime;

    public float time => Time.time - startTime;

    protected Core core;

    protected Animator Anim => core.animator;

    public StateMachine machine;

    public StateMachine parent;

    public State CurrentState => machine.state;

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
        CurrentState?.DoBranch();
    }

    public void DoFixedBranch()
    {
        DoFixed();
        CurrentState?.DoFixedBranch();
    }

    public void Initialise(StateMachine _parent)
    {
        parent = _parent;
        isComplete = false;
        startTime = Time.time;
    }
}

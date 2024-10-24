using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transition
{
    public delegate void ExecuteOnCompletion();
    public ExecuteOnCompletion executeOnCompletion;

    public GameObject id;
    public bool loop;

    public bool isRemoved;

    public float timer;
    public float timerMax;

    public virtual void Start()
    {

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
}

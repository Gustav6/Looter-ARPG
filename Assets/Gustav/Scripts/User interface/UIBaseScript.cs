using NaughtyAttributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UIBaseScript : MonoBehaviour
{
    protected Selectable SelectableScript {  get; private set; }

    public virtual void Start()
    {
        SelectableScript = GetComponent<Selectable>();
    }

    public virtual void Update()
    {
    }
}

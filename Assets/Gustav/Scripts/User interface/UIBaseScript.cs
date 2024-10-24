using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIBaseScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [field: SerializeField] public bool Active { get; private set; }
    [field: SerializeField] public Collider2D Collider { get; private set; }

    public virtual void Start()
    {
        ActiveStatus(false);
    }

    public virtual void Update()
    {

    }

    public void ActiveStatus(bool status)
    {
        Active = status;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ActiveStatus(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ActiveStatus(false);
    }
}

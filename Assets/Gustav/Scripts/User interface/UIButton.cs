using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIButton : UIBaseScript, IPointerClickHandler
{
    [SerializeField] private UnityEvent onClickEvent;

    public override void Start()
    {
        base.Start();
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.one;
    }

    private void RunOnActivation()
    {
        //ActivateSelectedFunctions();

        Debug.Log("PRESSED");
    }

    public void SwitchScene(int sceneBuildIndex)
    {
        SceneManager.LoadScene(sceneBuildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        Transition.ExecuteAfterTransition execute = null;

        if (onClickEvent != null)
        {
            execute += onClickEvent.Invoke;
        }

        TransitionSystem.AddTransition(new ScaleTransition(transform, 0.15f, new Vector3(1.05f, 1.05f), TransitionType.SmoothStop2, execute), gameObject);
    }

    public void ActivateMap()
    {
        if (MapManager.Instance == null)
        {
            return;
        }

        MapManager.Instance.StartCoroutine(MapManager.Instance.TryToLoadMap(MapManager.Instance.currentMap));
    }

    private enum Function
    {
        QuitGame,
        ChangeScene,
    }
}

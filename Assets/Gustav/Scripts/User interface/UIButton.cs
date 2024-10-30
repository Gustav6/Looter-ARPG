using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIButton : UIBaseScript, IPointerClickHandler
{
    private Dictionary<Function, Action> functionLookup;

    [BoxGroup("Button variables")]
    [SerializeField] private List<Function> actions = new();

    [BoxGroup("Button variables")]
    [Scene] [SerializeField] private string scene;

    public override void Start()
    {
        functionLookup = new Dictionary<Function, Action>()
        {
            { Function.QuitGame, Application.Quit },
            { Function.ChangeScene, SwitchScene },
        };

        base.Start();
    }

    private void RunOnActivation()
    {
        ActivateSelectedFunctions();

        Debug.Log("PRESSED");
    }

    private void ActivateSelectedFunctions()
    {
        foreach (Function function in actions)
        {
            functionLookup[function]?.Invoke();
        }
    }

    public void SwitchScene()
    {
        TransitionSystem.ClearTransitionList();
        SceneManager.LoadScene(scene);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        RunOnActivation();
    }

    private enum Function
    {
        QuitGame,
        ChangeScene,
    }
}

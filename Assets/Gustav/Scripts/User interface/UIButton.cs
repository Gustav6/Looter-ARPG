using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    [Header("Button variables")]
    [SerializeField] private List<Functions> actions = new();
    private Dictionary<Functions, Action> functionLookup;

    private void Start()
    {
        functionLookup = new Dictionary<Functions, System.Action>()
        {
            { Functions.QuitGame, Application.Quit },
        };
    }

    private void Update()
    {

    }

    public void ActivateSelectedFunctions()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            functionLookup[actions[i]]?.Invoke();
        }
    }

    private enum Functions
    {
        QuitGame,
    }
}

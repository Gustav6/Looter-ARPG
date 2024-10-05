using NaughtyAttributes;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    //[BoxGroup("Button variables")]

    [BoxGroup("Function variables")]
    [SerializeField] private List<Functions> actions = new();
    private Dictionary<Functions, Action> functionLookup;

    [BoxGroup("Function variables")]
    [Scene] [SerializeField] private string scene;

    [Foldout("Button references")]
    public TextMeshProUGUI textReference;
    [Foldout("Button references")]
    public Image backgroundReference;

    private void Start()
    {
        functionLookup = new Dictionary<Functions, Action>()
        {
            { Functions.QuitGame, Application.Quit },
            { Functions.ChangeScene, SwitchScene },
        };
    }

    private void ActivateSelectedFunctions()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            functionLookup[actions[i]]?.Invoke();
        }
    }

    public void SwitchScene()
    {
        SceneManager.LoadScene(scene);
    }

    private enum Functions
    {
        QuitGame,
        ChangeScene,
    }
}

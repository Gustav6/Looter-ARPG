using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateManager : MonoBehaviour
{
    public static UIStateManager Instance { get; private set; }

    public bool GamePaused { get; private set; }

    public float ResolutionScaling { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }

        ResolutionScaling = 1;
        UnPauseGame();
    }

    private void Update()
    {
        TransitionSystem.Update();
    }

    public void PauseGame()
    {
        GamePaused = true;
    }
    public void UnPauseGame()
    {
        GamePaused = false;
    }
}

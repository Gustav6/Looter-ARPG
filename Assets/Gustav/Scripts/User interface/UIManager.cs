using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public bool GamePaused { get; private set; }

    public bool FullScreen { get; private set; }
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
            Destroy(gameObject);
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

    public void DisableFullscreen()
    {
        FullScreen = false;
    }
    public void EnableFullscreen()
    {
        FullScreen = true;
    }
}

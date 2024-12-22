using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Dictionary<InstantiatedObjectType, GameObject> ObjectPairs { get; private set; }

    public InstantiateObjectOnStart[] instantiateOnStart;

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
        else
        {
            Destroy(gameObject);
            return;
        }

        ObjectPairs = new Dictionary<InstantiatedObjectType, GameObject>();

        foreach (InstantiateObjectOnStart objectToInstantiate in instantiateOnStart)
        {
            GameObject g = Instantiate(objectToInstantiate.prefab);
            g.SetActive(objectToInstantiate.active);

            ObjectPairs.TryAdd(objectToInstantiate.type, g);
        }

        ResolutionScaling = 1;
    }

    public void PowerupMenu(bool active)
    {
        ObjectPairs[InstantiatedObjectType.powerupCanvas].SetActive(active);
    }

    public void PauseMenu(bool active)
    {
        ObjectPairs[InstantiatedObjectType.pauseCanvas].SetActive(active);
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

[Serializable]
public struct InstantiateObjectOnStart
{
    public InstantiatedObjectType type;
    public GameObject prefab;
    public bool active;
}

public enum InstantiatedObjectType
{
    dialogueCanvas,
    powerupCanvas,
    pauseCanvas,
}
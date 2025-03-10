using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Dictionary<InstantiatedObjectType, GameObject> ObjectPairs { get; private set; }

    public InstantiateObjectOnStart[] instantiateOnStart;

    [field: SerializeField] public GameObject ActiveUIObject { get; private set; }

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

    public void ActivateANewUIObject(InstantiatedObjectType objectType)
    {
        if (ActiveUIObject != null)
        {
            ActiveUIObject.SetActive(false);
        }

        ActiveUIObject = ObjectPairs[objectType];
        ActiveUIObject.SetActive(true);
    }

    public void DeactivateCurrentUIObject()
    {
        if (ActiveUIObject == null)
        {
            return;
        }

        ActiveUIObject.SetActive(false);
        ActiveUIObject = null;
    }

    public void PauseFunction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GamePaused = !GamePaused;

            if (GamePaused)
            {
                if (ActiveUIObject != ObjectPairs[InstantiatedObjectType.pauseCanvas])
                {
                    ActivateANewUIObject(InstantiatedObjectType.pauseCanvas);
                }
            }
            else
            {
                if (ActiveUIObject == ObjectPairs[InstantiatedObjectType.pauseCanvas])
                {
                    DeactivateCurrentUIObject();
                }
            }
        }
    }

    public void UnPauseGame()
    {
        GamePaused = false;

        if (ActiveUIObject == ObjectPairs[InstantiatedObjectType.pauseCanvas])
        {
            DeactivateCurrentUIObject();
        }
    }
    public void PauseGame()
    {
        GamePaused = true;
        if (ActiveUIObject != ObjectPairs[InstantiatedObjectType.pauseCanvas])
        {
            ActivateANewUIObject(InstantiatedObjectType.pauseCanvas);
        }
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
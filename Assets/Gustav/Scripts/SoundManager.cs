using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public Dictionary<VolumeType, float> volumePairs;

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

        volumePairs = new()
        {
            { VolumeType.MasterVolume, 1 },
            { VolumeType.MusicVolume, 1 },
            { VolumeType.SFXVolume, 1 }
        };
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}

public enum VolumeType
{
    MasterVolume,
    MusicVolume,
    SFXVolume,
}

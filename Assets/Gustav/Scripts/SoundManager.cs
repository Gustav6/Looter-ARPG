using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private Dictionary<VolumeType, float> volumePairs;
    private float maxVolumeValue = 1, minVolumeValue = 0;

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

        volumePairs = new()
        {
            { VolumeType.MasterVolume, maxVolumeValue },
            { VolumeType.MusicVolume, maxVolumeValue },
            { VolumeType.SFXVolume, maxVolumeValue }
        };
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void SetVolume(VolumeType type, float value)
    {
        if (value > maxVolumeValue)
        {
            value = maxVolumeValue;
        }
        else if (value < minVolumeValue)
        {
            value = minVolumeValue;
        }

        if (volumePairs.ContainsKey(type))
        {
            volumePairs[type] = value;
        }
        else
        {
            volumePairs.Add(type, value);
        }
    }

    public float GetVolume(VolumeType type)
    {
        if (volumePairs.TryGetValue(type, out float volume))
        {
            return volume;
        }

        return 0;
    }
}

public enum VolumeType
{
    MasterVolume,
    MusicVolume,
    SFXVolume,
}

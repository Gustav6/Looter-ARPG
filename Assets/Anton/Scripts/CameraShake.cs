using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static float shakeIntensity;
    public static float shakeTime = 0.2f;

    public static float startIntensity;
    public static float timer;
    private CinemachineBasicMultiChannelPerlin _cbmcp;
    public static CinemachineVirtualCamera cinemachinVirtualCamera;


    void Start()
    {
        cinemachinVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        StopShake();
        startIntensity = shakeIntensity;
    }

    public static void ShakeCamera()
    {
        shakeIntensity = 40f;

        CinemachineBasicMultiChannelPerlin _cbmcp = cinemachinVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _cbmcp.m_AmplitudeGain = shakeIntensity;

        timer = shakeTime;
        shakeIntensity = startIntensity;
    }

    public static void StopShake()
    {
        CinemachineBasicMultiChannelPerlin _cbmcp = cinemachinVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _cbmcp.m_AmplitudeGain = 0f;

        timer = 0;
    }

    void Update()
    {

    }
}

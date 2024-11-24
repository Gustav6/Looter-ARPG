using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static float startIntensity;
    public static float shakeTimer = 0.5f;
    private CinemachineBasicMultiChannelPerlin _cbmcp;
    public static CinemachineVirtualCamera cinemachinVirtualCamera;


    void Start()
    {
        cinemachinVirtualCamera = GetComponent<CinemachineVirtualCamera>();   
    }

    public static void ShakeCamera(float intensity)
    {
        CinemachineBasicMultiChannelPerlin _cbmcp = cinemachinVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _cbmcp.m_AmplitudeGain = intensity;
    }

    public static void StopShakeCamera()
    {
        shakeTimer -= Time.deltaTime;
        if (shakeTimer <= 0)
        {
            CinemachineBasicMultiChannelPerlin _cbmcp = cinemachinVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _cbmcp.m_AmplitudeGain = 0f;
        }  
    }

    void Update()
    {
        
    }
}

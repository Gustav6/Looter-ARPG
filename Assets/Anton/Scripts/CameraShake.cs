using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static float startIntensity;
    private CinemachineBasicMultiChannelPerlin _cbmcp;
    public static CinemachineVirtualCamera cinemachinVirtualCamera;     

    void Start()
    {
        cinemachinVirtualCamera = GetComponent<CinemachineVirtualCamera>();   
    }

    public static void ShakeCamera(float intensity)
    {
        CinemachineBasicMultiChannelPerlin _cbmcp = cinemachinVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _cbmcp.m_AmplitudeGain = intensity * (GunController.Instance.fireForce / 20);
    }

    public static void StopShakeCamera()
    {      
       CinemachineBasicMultiChannelPerlin _cbmcp = cinemachinVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _cbmcp.m_AmplitudeGain = 0f;     
    }
}

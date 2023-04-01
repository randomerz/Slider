using Cinemachine;
using UnityEngine;

public class AudioCameraBridge : MonoBehaviour
{
    public static void UpdateCameraBrain(CinemachineBrain brain) => AudioManager.UpdateCamera(brain);
}

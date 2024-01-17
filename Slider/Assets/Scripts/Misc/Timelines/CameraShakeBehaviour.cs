using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class CameraShakeBehaviour : PlayableBehaviour
{
    public float intensity = 1.0f;
    public ShakeType shakeType = ShakeType.decrease;

    public enum ShakeType 
    {
        decrease,
        constant,
        increase,
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!Application.isPlaying)
        {
            base.OnBehaviourPlay(playable, info);
            return;
        }
        
        // How to get time in timeline
        // playable.GetGraph().GetRootPlayable(0).GetTime()
        // https://forum.unity.com/threads/how-to-get-playable-gettime-when-not-in-play-mode.496047/

        DoShake(intensity, (float)playable.GetDuration(), shakeType);

        base.OnBehaviourPlay(playable, info);
    }

    private void DoShake(float intensity, float duration, ShakeType type)
    {
        // Debug.Log($"Playing camera shake with intensity {intensity}, duration {duration}, type {type}");
        switch (type)
        {
            case ShakeType.decrease:
                CameraShake.Shake(duration, intensity);
                break;
            case ShakeType.constant:
                CameraShake.ShakeConstant(duration, intensity);
                break;
            case ShakeType.increase:
                CameraShake.ShakeIncrease(duration, intensity);
                break;
        }
    }
}
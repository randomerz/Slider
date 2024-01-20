using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class CameraShakeAsset : PlayableAsset
{
    public float intensity = 1.0f;
    public CameraShakeBehaviour.ShakeType shakeType = CameraShakeBehaviour.ShakeType.decrease;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CameraShakeBehaviour>.Create(graph);

        CameraShakeBehaviour cameraShakeBehaviour = playable.GetBehaviour();
        cameraShakeBehaviour.intensity = intensity;
        cameraShakeBehaviour.shakeType = shakeType;

        return playable;
    }
}
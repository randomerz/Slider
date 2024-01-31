using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AudioAsset : PlayableAsset
{
    public string soundName = "Audio Name";
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;
    [Range(0.3f, 3.0f)]
    public float pitch = 1.0f;

    // https://forum.unity.com/threads/changing-duration-of-custom-playable-clip.545475/
    // This doesn't even really work but whatever
    public override double duration { get { return 0.25f; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<AudioBehaviour>.Create(graph);

        AudioBehaviour audioBehaviour = playable.GetBehaviour();
        audioBehaviour.soundName = soundName;
        audioBehaviour.volume = volume;
        audioBehaviour.pitch = pitch;

        return playable;
    }
}
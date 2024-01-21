using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AudioBehaviour : PlayableBehaviour
{
    public string soundName = "Audio Name";
    public float volume = 1.0f;
    public float pitch = 1.0f;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!Application.isPlaying)
        {
            base.OnBehaviourPlay(playable, info);
            return;
        }

        PlaySound();

        base.OnBehaviourPlay(playable, info);
    }

    private void PlaySound()
    {
        AudioManager.PickSound(soundName)
                    .WithVolume(volume)
                    .WithPitch(pitch)
                    .WithAttachmentToTransform(Player.GetInstance().transform)
                    .AndPlay();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class BoatParticleEffectController : MonoBehaviour
{
    public ParticleSystem particles;

    public float minParticleScore = 2; // cumulative distance until spawning a particle
    public float maxParticleScore = 3;
    public float timeScoreMultiplier = 0.2f;
    private float targetScore; // spawn particle when = 0

    private float numberParticlesUntilSFX = 3;
    private float sfxCounter = 0;

    private Vector3 lastMoveDir;
    private Vector3 lastPlayerPos;
    private Vector3 lastPlayerLocalPos;
    
    private void Start() 
    {
        targetScore = Random.Range(minParticleScore, maxParticleScore);
        lastMoveDir = Player.GetLastMoveDirection();
        lastPlayerPos = Player.GetPosition();
        lastPlayerLocalPos = Player.GetInstance().transform.localPosition;
    }

    void Update()
    {
        if (!Player.GetInstance().GetIsOnWater())
            return;

        UpdateParticlePosition();

        Vector3 curMoveDir = Player.GetLastMoveDirection(); // is actually last

        if (curMoveDir == -lastMoveDir && curMoveDir != Vector3.zero)
        {
            targetScore -= 1;
        }
        else
        {
            float distTravelled = Vector3.Distance(lastPlayerPos, Player.GetPosition());
            if (Player.GetInstance().GetSTileUnderneath() != null && Player.GetInstance().GetSTileUnderneath().IsMoving())
            {
                // distTravelled = Vector3.Distance(lastPlayerLocalPos, Player.GetInstance().transform.localPosition);
                distTravelled = -Time.deltaTime * timeScoreMultiplier; // do nothing bc it looks weird when the particles arent parented to the slider...
            }
            targetScore -= distTravelled;
        }

        targetScore -= Time.deltaTime * timeScoreMultiplier;

        if (targetScore <= 0)
        {
            SpawnBoatRipple();
        }

        lastPlayerPos = Player.GetPosition();
        lastPlayerLocalPos = Player.GetInstance().transform.localPosition;
        SetLastMoveDir(curMoveDir);
    }

    private void UpdateParticlePosition()
    {
        transform.SetParent(Player.GetInstance().transform.parent);
        transform.position = Player.GetPosition() + new Vector3(0, -0.5f);
    }

    private void SetLastMoveDir(Vector3 moveDir)
    {
        lastMoveDir = moveDir;
        particles.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg);
    }

    private void SpawnBoatRipple()
    {
        particles.Play();

        HandleAudio();

        targetScore = Random.Range(minParticleScore, maxParticleScore);
    }

    private void HandleAudio()
    {
        sfxCounter += 1;

        AudioManager.PlayWithVolume("Boat Splash", sfxCounter >= numberParticlesUntilSFX ? 1 : 0.25f);
        if (sfxCounter >= numberParticlesUntilSFX)
        {
            sfxCounter = 0;
        }
    }
}

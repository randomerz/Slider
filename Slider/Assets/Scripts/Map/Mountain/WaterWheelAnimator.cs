using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWheelAnimator : MonoBehaviour
{
    public Animator animatorMain;
    public Animator animatorWater;
    public SpriteRenderer waterSpriteRenderer;
    public Animator animatorWorm;
    public Animator animatorGenerator;
    public List<WW_GearAnimator> gearAnimators;
    public GameObject waterWheelAmbienceSource;

    private const float WW_SLOW_SPEED = 0.01f;
    private const float WW_NORMAL_SPEED = 1.0f;
    private const float WW_SPLASH_CUTOFF = 0.25f;

    public string fullSpeedGridRegex = "(3._..|.._3.)_.._..";
    private float targetAnimationSpeed = WW_NORMAL_SPEED;
    private float currentAnimationSpeed = WW_NORMAL_SPEED;
    private const float TRACKING_SPEED = 0.25f;

    private bool isGear2Frozen = false;
    private bool isGear4Frozen = false;

    private void Awake() 
    {
        for (int i = 0; i < gearAnimators.Count - 1; i++)
        {
            gearAnimators[i].nextGear = gearAnimators[i + 1];
        }
    }

    void Start()
    {
        UpdateAnimationSpeedTarget();
        currentAnimationSpeed = targetAnimationSpeed;
        SetAnimationSpeed(currentAnimationSpeed);

        // TODO: handle gear on start stuff here
        UpdateGears();
    }

    void Update()
    {
        UpdateAnimationSpeedTarget();

        if (currentAnimationSpeed != targetAnimationSpeed)
        {
            if (currentAnimationSpeed < targetAnimationSpeed)
            {
                currentAnimationSpeed = Mathf.Clamp(currentAnimationSpeed + Time.deltaTime * TRACKING_SPEED, WW_SLOW_SPEED, targetAnimationSpeed);
            }
            else
            {
                currentAnimationSpeed = Mathf.Clamp(currentAnimationSpeed - Time.deltaTime * TRACKING_SPEED, targetAnimationSpeed, WW_NORMAL_SPEED);
            }
            SetAnimationSpeed(currentAnimationSpeed);
        }

        waterWheelAmbienceSource.SetActive(IsFullSpeed());
        
        // Optional: change targetAnimationSpeed to currentAnimationSpeed
        animatorGenerator.SetBool("isOn", IsGeneratorOn());
    }

    private bool IsGeneratorOn()
    {
        return targetAnimationSpeed == 1 && !isGear2Frozen && !isGear4Frozen;
    }

    private bool IsFullSpeed()
    {
        return targetAnimationSpeed == 1 && !isGear2Frozen && !isGear4Frozen;
    }

    private void UpdateAnimationSpeedTarget()
    {
        float newSpeed = CheckGrid.contains(SGrid.GetGridString(), fullSpeedGridRegex) ? WW_NORMAL_SPEED : WW_SLOW_SPEED;
        targetAnimationSpeed = newSpeed;
    }

    private void SetAnimationSpeed(float speed)
    {
        animatorMain.SetFloat("Speed", speed);
        animatorWater.SetFloat("Speed", speed);

        float gearSpeed = Map(WW_SLOW_SPEED, WW_NORMAL_SPEED, 0, 1, speed);
        animatorWorm.SetFloat("Speed", gearSpeed);

        foreach (WW_GearAnimator ga in gearAnimators)
        {
            ga.SetSpeed(gearSpeed);
        }

        Color c = waterSpriteRenderer.color;
        if (speed >= WW_SPLASH_CUTOFF)
        {
            c.a = 1;
        }
        else
        {
            float alpha = Mathf.InverseLerp(0, WW_SPLASH_CUTOFF, speed);
            c.a = alpha;
        }
        waterSpriteRenderer.color = c;
    }

    private float Map(float a, float b, float x, float y, float value)
    {
        return Mathf.Lerp(x, y, Mathf.InverseLerp(a, b, value));
    }

    private void UpdateGears()
    {
        if (!isGear2Frozen && !isGear4Frozen)
        {
            // No frozen gears => all move

            foreach (WW_GearAnimator ga in gearAnimators)
            {
                ga.SetCurrentState(WW_GearAnimator.State.Normal);
            }
        }
        else
        {
            // Otherwise a gear is frozen. Below is for the first time setting it to frozen. 
            // - Normal should become Clicking up till the frozen gear, then NotMoving
            // - If the first is already Clicking, then the gears should update themselves

            animatorGenerator.SetBool("isOn", false);

            if (gearAnimators.Count == 0) 
            {
                return;
            }

            // if (gearAnimators[0].state == WW_GearAnimator.State.Clicking)
            // {
            //     return;
            // }

            bool passedFrozenGear = false;
            for (int i = 0; i < gearAnimators.Count; i++)
            {

                WW_GearAnimator ga = gearAnimators[i];

                if ((i == 2 && isGear2Frozen) || (i == 4 && isGear4Frozen))
                {
                    ga.SetCurrentState(WW_GearAnimator.State.Frozen);
                    passedFrozenGear = true;
                    continue;
                }

                // if (gearAnimators[0].state == WW_GearAnimator.State.Clicking)
                // {
                //     continue;
                // }

                if (!passedFrozenGear)
                {
                    ga.SetCurrentState(WW_GearAnimator.State.Clicking);
                }
                else
                {
                    ga.SetCurrentState(WW_GearAnimator.State.NotMoving);
                }
            }
        }
    }

    public void SetGear2Frozen(bool isFrozen)
    {
        isGear2Frozen = isFrozen;
        if (gearAnimators.Count > 2)
        {
            gearAnimators[2].SetCurrentState(isFrozen ? WW_GearAnimator.State.Frozen : WW_GearAnimator.State.NotMoving);
        }
        UpdateGears();
    }

    public void SetGear4Frozen(bool isFrozen)
    {
        isGear4Frozen = isFrozen;
        if (gearAnimators.Count > 4)
        {
            gearAnimators[4].SetCurrentState(isFrozen ? WW_GearAnimator.State.Frozen : WW_GearAnimator.State.NotMoving);
        }
        UpdateGears();
    }
}

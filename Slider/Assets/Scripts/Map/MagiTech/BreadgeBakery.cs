using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadgeBakery : MonoBehaviour
{
    public const string BREADGE_HINT_SAVE_STRING = "MagiTechBreadgeHint";
    public const string BREADGE_AMOUNT_SAVE_STRING = "MagiTechBreadgeAmount";
    public const string RAINBOW_BREADGE_ACQUIRED = "MagiTechRainbowBreadgeAcquired";
    private int breadgeAmount;
    
    private bool isPlayingCutscene;

    private const float CIRCLE_RADIUS = 1.75f;
    private const float SLOW_ROTATION_SPEED = -20f; // angle degrees per second
    private const float FAST_ROTATION_SPEED = -60f; 
    private float rotationSpeed;
    private List<ManagedBreadgeObject> managedBreadgeObjects = new();
    
    [SerializeField] private List<SpriteRenderer> breadgeObjects = new();
    [SerializeField] private GameObject rainbowBreadge;
    [SerializeField] private Collectible rainbowBreadgeCollectible;
    [SerializeField] private AnimationCurve breadgeStartCurve;
    [SerializeField] private AnimationCurve breadgeEndCurve;

    private class ManagedBreadgeObject 
    {
        public int index;
        public bool isActive;

        public float currentAngle; // degrees
        public float angleOffset;
        public float currentRadius;

        public Vector3 CalculateTotalOffset() 
        {
            float angleRadians = (currentAngle + angleOffset) * Mathf.Deg2Rad;
            return currentRadius * new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
        }
    }

    private void Awake()
    {
        int num = breadgeObjects.Count;
        for (int i = 0; i < num; i++)
        {
            managedBreadgeObjects.Add(new ManagedBreadgeObject {
                index = i,
                isActive = false,
                currentAngle = ((360 / num) * 2 * i) % 360,
                angleOffset = 0,
                currentRadius = CIRCLE_RADIUS
            });
        }
        rotationSpeed = SLOW_ROTATION_SPEED;
    }

    private void Start()
    {
        Update();

        if (SaveSystem.Current.GetBool(RAINBOW_BREADGE_ACQUIRED))
        {
            rainbowBreadge.SetActive(true);
        }
    }

    private void Update()
    {
        UpdateBreadgeHint();

        foreach (ManagedBreadgeObject b in managedBreadgeObjects)
        {
            UpdateManagedBreadge(b);
        }
    }

    private void UpdateBreadgeHint()
    {
        List<string> missingAreas = new();

        foreach (Area area in Enum.GetValues(typeof(Area)))
        {
            if (!PlayerInventory.Contains("Breadge", area))
            {
                missingAreas.Add(Areas.GetDisplayName(area));
            }
        }

        breadgeAmount = 9 - missingAreas.Count;
        SaveSystem.Current.SetString(BREADGE_AMOUNT_SAVE_STRING, (9 - missingAreas.Count).ToString());
        if (breadgeAmount == 9)
        {
            SaveSystem.Current.SetString(BREADGE_HINT_SAVE_STRING, "Woah, you have them all!");
        }
        else if (breadgeAmount == 0)
        {
            SaveSystem.Current.SetString(BREADGE_HINT_SAVE_STRING, "You're missing... all of them!");
        }
        else if (breadgeAmount == 8)
        {
            if (missingAreas.Count == 0)
            {
                SaveSystem.Current.SetString(BREADGE_HINT_SAVE_STRING, $"You should only be missing one but I'm not sure where -- something went wrong tell the devs!");
                return;
            }
            SaveSystem.Current.SetString(BREADGE_HINT_SAVE_STRING, $"You're only missing the one from {missingAreas[0]}!");
        }
        else
        {
            string allAreas = string.Join(", ", missingAreas);
            SaveSystem.Current.SetString(BREADGE_HINT_SAVE_STRING, $"You're missing: {allAreas}.");
        }
    }

    private void UpdateManagedBreadge(ManagedBreadgeObject managedBreadge)
    {
        managedBreadge.currentAngle += rotationSpeed * Time.deltaTime;
        Vector3 offset = managedBreadge.CalculateTotalOffset();
        breadgeObjects[managedBreadge.index].transform.position = transform.position + offset;
    }

    public void DoBreadgeCombination()
    {
        if (isPlayingCutscene)
            return;
        isPlayingCutscene = true;

        StartCoroutine(_DoBreadgeCombination());
    }

    private IEnumerator _DoBreadgeCombination()
    {
        AudioManager.DampenMusic(this, 0.75f, 1.75f + (9 * 1.5f) + 2 + 0.75f + 1.5f);

        CameraShake.Shake(1, 0.25f);
        AudioManager.Play("MagicChimes1");

        yield return new WaitForSeconds(1.75f);

        for (int i = 0; i < breadgeObjects.Count; i++)
        {
            breadgeObjects[i].gameObject.SetActive(true);
            AudioManager.PlayWithPitch("UI Click", 0.7f + (0.02f * i));

            CoroutineUtils.ExecuteEachFrame(
                (x) => {
                    managedBreadgeObjects[i].angleOffset = Mathf.Lerp(45, 0, x);
                    managedBreadgeObjects[i].currentRadius = Mathf.Lerp(0, CIRCLE_RADIUS, x);
                    Color c = breadgeObjects[i].color;
                    c.a = Mathf.Lerp(0, 1, x);
                    breadgeObjects[i].color = c;
                },
                () => {
                    managedBreadgeObjects[i].angleOffset = 0;
                    managedBreadgeObjects[i].currentRadius = CIRCLE_RADIUS;
                    Color c = breadgeObjects[i].color;
                    c.a = 1;
                    breadgeObjects[i].color = c;
                },
                this, 
                1, 
                breadgeStartCurve
            );

            yield return new WaitForSeconds(1.5f);
        }

        float circlingDuration = 2;
        CoroutineUtils.ExecuteEachFrame(
            (x) => {
                rotationSpeed = Mathf.Lerp(SLOW_ROTATION_SPEED, FAST_ROTATION_SPEED, x);
            },
            () => {
                rotationSpeed = FAST_ROTATION_SPEED;
            },
            this, 
            circlingDuration
        );

        yield return new WaitForSeconds(circlingDuration);

        // TODO: 2s White noise riser sound

        for (int i = 0; i < breadgeObjects.Count; i++)
        {
            breadgeObjects[i].gameObject.SetActive(true);

            // Create a new variable bc lambdas
            int index = i;

            CoroutineUtils.ExecuteEachFrame(
                (x) => {
                    // The curves go from 1 to 0 here
                    managedBreadgeObjects[index].angleOffset = Mathf.Lerp(-45, 0, x);
                    managedBreadgeObjects[index].currentRadius = Mathf.Lerp(0, CIRCLE_RADIUS, x);
                },
                () => {
                    managedBreadgeObjects[index].angleOffset = -45;
                    managedBreadgeObjects[index].currentRadius = 0;
                },
                this, 
                0.75f, 
                breadgeEndCurve
            );
        }

        yield return new WaitForSeconds(0.75f);

        UIEffects.FlashWhite(callbackMiddle: null, callbackEnd: null, speed: 4);
        ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position);
        AudioManager.Play("Hat Click");
        
        for (int i = 0; i < breadgeObjects.Count; i++)
        {
            breadgeObjects[i].gameObject.SetActive(false);
        }
        // just leave it on and floating
        rainbowBreadge.SetActive(true);
        SaveSystem.Current.SetBool(RAINBOW_BREADGE_ACQUIRED, true);

        yield return new WaitForSeconds(1.5f);

        rainbowBreadgeCollectible.DoPickUp();
        
        isPlayingCutscene = false;
    }

    private bool DEBUG_BREADGE = false;
    public void DEBUG_ACTIVATE_ALL_BREADGE()
    {
        DEBUG_BREADGE = true;
    }

    public void Has0Breadge(Condition c) => c.SetSpec(breadgeAmount == 0);
    public void Has1BreadgeAtLeast(Condition c) => c.SetSpec(breadgeAmount >= 1);
    public void Has9Breadge(Condition c) => c.SetSpec(breadgeAmount == 9 || DEBUG_BREADGE);
}
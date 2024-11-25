using UnityEngine;
using UnityEngine.Serialization;

public class UITrackerMilitaryUnit : UITracker
{
    private MilitaryUnit _targetUnit;
    public MilitaryUnit TargetUnit {
        get {
            if (_targetUnit == null)
            {
                if (target.TryGetComponent<MilitaryUnit>(out MilitaryUnit mu))
                {
                    _targetUnit = mu;
                }
                else if (target.TryGetComponent<MilitaryNPCController>(out MilitaryNPCController mnpc))
                {
                    _targetUnit = mnpc.militaryUnit;
                }
            }
            return _targetUnit;
        }
        set => _targetUnit = value;
    }
    private bool isTargetAlien;

    public RectTransform rectTransform;
    public RectTransform imageRectTransform;
    public AnimationCurve transitionOffsetCurve;
    private Coroutine coroutine;

    private void Start()
    {
        if (target == null || TargetUnit == null)
        {
            Debug.LogWarning($"[Military] Target does not exist but tracker still exists.");
            Destroy(gameObject);
            return;
        }
        isTargetAlien = TargetUnit.UnitTeam == MilitaryUnit.Team.Alien;
    }

    private void Update()
    {
        if (isTargetAlien && coroutine == null)
        {
            imageRectTransform.anchoredPosition = GetBasePosition();
        }
    }

    public void AnimateImageFrom(Vector2 offset)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        coroutine = CoroutineUtils.ExecuteEachFrame(
            (x) => {
                imageRectTransform.anchoredPosition = Vector2.Lerp(offset, GetBasePosition(), x);
            },
            () => {
                imageRectTransform.anchoredPosition = GetBasePosition();
                coroutine = null;
            },
            this,
            0.5f,
            transitionOffsetCurve
        );
    }

    public Vector2 GetBasePosition()
    {
        if (isTargetAlien)
        {
            int t = (int)Time.time;
            return t % 2 == 0 ? Vector2.zero : Vector2.up;
        }

        return Vector2.zero;
    }
}
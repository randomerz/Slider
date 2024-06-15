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

    public RectTransform rectTransform;
    public RectTransform imageRectTransform;
    public AnimationCurve transitionOffsetCurve;
    private Coroutine coroutine;

    public void AnimateImageFrom(Vector2 offset)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        coroutine = CoroutineUtils.ExecuteEachFrame(
            (x) => {
                imageRectTransform.anchoredPosition = Vector2.Lerp(offset, Vector2.zero, x);
            },
            () => {
                imageRectTransform.anchoredPosition = Vector2.zero;
                coroutine = null;
            },
            this,
            0.5f,
            transitionOffsetCurve
        );
    }
}
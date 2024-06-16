using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryUITrackerManager : UITrackerManager
{
    protected override Vector2 center => new Vector2(19.5f, 19.5f);
    protected override float centerScale => 2; // idk why 2 looks best

    [Header("Military")]
    public Vector2 topRightOffMapCorner;
    public GameObject militaryUITrackerPrefab;
    public GameObject militaryUIFightTrackerPrefab;
    private static MilitarySpriteTable _militarySpriteTableCache;
    private static MilitarySpriteTable MilitarySpriteTable {
        get {
            if (_militarySpriteTableCache == null)
            {
                _militarySpriteTableCache = (MilitaryGrid.Current as MilitaryGrid).militarySpriteTable;
            }
            return _militarySpriteTableCache;
        }
    }

    private static List<MilitaryUnit> unitTrackerBuffer = new();

    protected override void Awake()
    {
        base.Awake();

        foreach (MilitaryUnit unit in unitTrackerBuffer)
        {
            AddUnitTracker(unit);
        }
        unitTrackerBuffer.Clear();
    }

    public static void AddUnitTracker(MilitaryUnit unit)
    {
        if (_instance == null)
        {
            unitTrackerBuffer.Add(unit);
            return;
        }
        
        GameObject go = Instantiate((_instance as MilitaryUITrackerManager).militaryUITrackerPrefab, _instance.transform);
        UITracker uiTracker = go.GetComponent<UITracker>();
        uiTracker.target = unit.NPCController.gameObject;
        uiTracker.SetSprite(MilitarySpriteTable.GetUIIconForUnit(unit));
        
        AddNewCustomTracker(uiTracker, unit.NPCController.gameObject);
    }

    public static void AddFightTracker(GameObject fightGameObject)
    {
        if (_instance == null)
        {
            Debug.Log($"Skipped adding fight tracker.");
            return;
        }
        GameObject go = Instantiate((_instance as MilitaryUITrackerManager).militaryUIFightTrackerPrefab, _instance.transform);
        UITracker uiTracker = go.GetComponent<UITracker>();
        AddNewCustomTracker(uiTracker, fightGameObject);
    }

    public static void RemoveUnitTracker(MilitaryUnit unit)
    {
        RemoveTracker(unit.NPCController.gameObject);
    }

    protected override void UpdateTrackerPostion(UITracker tracker)
    {
        if (tracker is not UITrackerMilitaryUnit militaryTracker)
        {
            base.UpdateTrackerPostion(tracker);
            return;
        }
        
        Vector2Int gridPos = militaryTracker.TargetUnit.GridPosition;
        Vector2 oldPos = tracker.transform.position;
        bool shouldAnimateMove = militaryTracker.transform.position != transform.position; // make sure they arent both in the center
        if (0 <= gridPos.x && gridPos.x < SGrid.Current.Width && 0 <= gridPos.y && gridPos.y < SGrid.Current.Height)
        {
            // In bounds
            ArtifactTileButton button = UIArtifact.GetButton(gridPos.x, gridPos.y);

            if (militaryTracker.TargetUnit.AttachedSTile != null && oldPos != Vector2.zero)
            {
                STile s = militaryTracker.TargetUnit.AttachedSTile;
                ArtifactTileButton otherButton = UIArtifact.GetInstance().GetButton(s.islandId);
                
                militaryTracker.rectTransform.SetParent(otherButton.imageRectTransform);
                militaryTracker.rectTransform.anchoredPosition = Vector2.zero;
                // if (s.islandId == button.islandId && shouldAnimateMove)
                // {
                //     Debug.Log($"switch button");
                //     shouldAnimateMove = false;
                //     militaryTracker.rectTransform.SetParent(otherButton.imageRectTransform);
                //     militaryTracker.rectTransform.anchoredPosition = Vector2.zero;
                // }
                // else
                // {
                //     Debug.Log($"attached to button {oldPos}");
                //     militaryTracker.rectTransform.SetParent(button.imageRectTransform);
                //     militaryTracker.rectTransform.anchoredPosition = Vector2.zero;
                // }
                
            }
            else
            {
                militaryTracker.rectTransform.SetParent(button.imageRectTransform);
                militaryTracker.rectTransform.anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            // Not on a stile
            militaryTracker.rectTransform.SetParent(artifactPanel.GetComponent<RectTransform>());
            Vector2 pos = Vector2.zero;

            if (0 <= gridPos.x && gridPos.x < SGrid.Current.Width)
            {
                ArtifactTileButton button = UIArtifact.GetButton(gridPos.x, 0);
                pos.x = button.GetComponent<RectTransform>().anchoredPosition.x;
            }
            else if (gridPos.x < 0)
            {
                pos.x = -topRightOffMapCorner.x;
            }
            else
            {
                pos.x = topRightOffMapCorner.x;
            }

            if (0 <= gridPos.y && gridPos.y < SGrid.Current.Height)
            {
                ArtifactTileButton button = UIArtifact.GetButton(0, gridPos.y);
                pos.y = button.GetComponent<RectTransform>().anchoredPosition.y;
            }
            else if (gridPos.y < 0)
            {
                pos.y = -topRightOffMapCorner.y;
            }
            else
            {
                pos.y = topRightOffMapCorner.y;
            }

            militaryTracker.rectTransform.anchoredPosition = pos;
        }

        Vector2 newPos = tracker.transform.position;
        if (shouldAnimateMove && newPos != oldPos)
        {
            militaryTracker.AnimateImageFrom((oldPos - newPos).normalized);
        }
    }
}

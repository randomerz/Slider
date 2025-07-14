using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MountainUINavMapper : MonoBehaviour {
    public Button leftButton;
    public Button rightButton;
    
    private readonly Dictionary<Vector2Int, Button> coordToButtonCache = new();

    private struct SelectData {
        public Vector2Int selectOnUp;
        public Vector2Int selectOnDown;
        public Vector2Int selectOnLeft;
        public Vector2Int selectOnRight;
    }

    private static readonly Vector2Int NO_BUTTON = new Vector2Int(-1, 0);
    private static readonly Vector2Int LEFT_ARROW_BUTTON = new Vector2Int(-1, 1);
    private static readonly Vector2Int RIGHT_ARROW_BUTTON = new Vector2Int(-1, 2);

    private readonly Dictionary<Vector2Int, SelectData> selectDataMap = new() {
        {
            new Vector2Int(0, 0),
            new SelectData {
                selectOnUp    = new Vector2Int(0, 2),
                selectOnDown  = new Vector2Int(1, 0),
                selectOnLeft  = LEFT_ARROW_BUTTON,
                selectOnRight = new Vector2Int(0, 1),
            }
        },
        {
            new Vector2Int(1, 0),
            new SelectData {
                selectOnUp    = new Vector2Int(0, 1),
                selectOnDown  = NO_BUTTON,
                selectOnLeft  = new Vector2Int(0, 0),
                selectOnRight = new Vector2Int(1, 1),
            }
        },
        {
            new Vector2Int(0, 1),
            new SelectData {
                selectOnUp    = new Vector2Int(1, 2),
                selectOnDown  = new Vector2Int(1, 0),
                selectOnLeft  = new Vector2Int(0, 0),
                selectOnRight = new Vector2Int(1, 1),
            }
        },
        {
            new Vector2Int(1, 1),
            new SelectData {
                selectOnUp    = new Vector2Int(1, 3),
                selectOnDown  = new Vector2Int(1, 0),
                selectOnLeft  = new Vector2Int(0, 1),
                selectOnRight = RIGHT_ARROW_BUTTON,
            }
        },
        {
            new Vector2Int(0, 2),
            new SelectData {
                selectOnUp    = new Vector2Int(0, 3),
                selectOnDown  = new Vector2Int(0, 0),
                selectOnLeft  = LEFT_ARROW_BUTTON,
                selectOnRight = new Vector2Int(1, 2),
            }
        },
        {
            new Vector2Int(1, 2),
            new SelectData {
                selectOnUp    = new Vector2Int(0, 3),
                selectOnDown  = new Vector2Int(0, 1),
                selectOnLeft  = new Vector2Int(0, 2),
                selectOnRight = new Vector2Int(1, 3),
            }
        },
        {
            new Vector2Int(0, 3),
            new SelectData {
                selectOnUp    = NO_BUTTON,
                selectOnDown  = new Vector2Int(1, 2),
                selectOnLeft  = new Vector2Int(0, 2),
                selectOnRight = new Vector2Int(1, 3),
            }
        },
        {
            new Vector2Int(1, 3),
            new SelectData {
                selectOnUp    = new Vector2Int(0, 3),
                selectOnDown  = new Vector2Int(1, 1),
                selectOnLeft  = new Vector2Int(1, 2),
                selectOnRight = RIGHT_ARROW_BUTTON,
            }
        },
    };

    void LateUpdate()
    {
        foreach (Vector2Int key in selectDataMap.Keys)
        {
            coordToButtonCache[key] = UIArtifact.GetButton(key.x, key.y).GetComponent<Button>();
        }

        foreach (Vector2Int key in selectDataMap.Keys)
        {
            SelectData data = selectDataMap[key];
            Button button = coordToButtonCache[key];
            Navigation nav = button.navigation;
            nav = UpdateNav(nav, data);
            button.navigation = nav;
        }
    }

    private Navigation UpdateNav(Navigation navigation, SelectData selectData)
    {
        navigation.selectOnUp = GetNavigationButton(selectData.selectOnUp);
        navigation.selectOnDown = GetNavigationButton(selectData.selectOnDown);
        navigation.selectOnLeft = GetNavigationButton(selectData.selectOnLeft);
        navigation.selectOnRight = GetNavigationButton(selectData.selectOnRight);
        return navigation;
    }

    private Button GetNavigationButton(Vector2Int coord)
    {
        if (coord == NO_BUTTON)
            return null;
        if (coord == LEFT_ARROW_BUTTON)
            return leftButton.isActiveAndEnabled ? leftButton : null;
        if (coord == RIGHT_ARROW_BUTTON)
            return rightButton.isActiveAndEnabled ? rightButton : null;

        if (coordToButtonCache.TryGetValue(coord, out Button button))
            return button;

        return null;
    }
}
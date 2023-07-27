using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class MGUISquare : MonoBehaviour
{
    public enum AnimStates
    {
        EMPTY,
        MOVE,
        BATTLE
    }

    public static Dictionary<AnimStates, string> AnimNames = new()
    {
        { AnimStates.EMPTY, "MGUI_Empty"},
        { AnimStates.MOVE, "MGUI_Move" },
        { AnimStates.BATTLE, "MGUI_Battle" }
    };

    [SerializeField] private int x;
    [SerializeField] private int y;

    private Animator _anim;
    private MGUI _ui;

    //private MGSpace _mgSpace;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _ui = GetComponentInParent<MGUI>();
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(x, y);
    }

    public void Select()
    {
        _ui.SelectSquare(this);
    }

    public void ChangeAnimState(AnimStates state)
    {
        _anim.Play(AnimNames[state]);
    }
}

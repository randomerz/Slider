using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[CustomEditor(typeof(NPCEmotes))]
public class NPCEmotesEditor : Editor
{
    private NPCEmotes _target;

    private void OnEnable()
    {
        _target = (NPCEmotes)target;
        _target.spriteRenderer.enabled = true;
    }

    private void OnDisable() 
    {
        _target.spriteRenderer.enabled = false;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button($"(Toggle) NPC Default Faces: {(_target.npcDefaultFacesRight ? "Right" : "Left")}") && GUI.changed)
        {
            _target.xOffset = Mathf.Abs(_target.transform.localPosition.x);
            _target.npcDefaultFacesRight = !_target.npcDefaultFacesRight;
            _target.UpdateEmotePosition();
        }

        base.OnInspectorGUI();
    }
}

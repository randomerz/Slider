using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[CustomEditor(typeof(NPC))]
public class NPCEditor : Editor
{
    private NPC _target;

    private void OnEnable()
    {
        _target = (NPC)target;
        SetConditionalNames();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Custom tools", EditorStyles.boldLabel);

        _target.autoSetWaitUntilPlayerAction = EditorGUILayout.Toggle("Automatically Set 'waitUntilPlayerAction'", _target.autoSetWaitUntilPlayerAction);
        if (_target.autoSetWaitUntilPlayerAction && GUI.changed)
        {
            SetWaitUntilPlayerActions();
        }


    }

    private void SetWaitUntilPlayerActions()
    {
        foreach (NPCConditionals npcConditional in _target.Conds)
        {
            for (int i = 0; i < npcConditional.dialogueChain.Count; i++)
            {
                DialogueData dialogueData = npcConditional.dialogueChain[i];
                bool isLast = i == npcConditional.dialogueChain.Count - 1;
                bool dontInterrupt = dialogueData.dontInterrupt;
                bool advanceDialogueManually = dialogueData.advanceDialogueManually;
                npcConditional.dialogueChain[i].waitUntilPlayerAction = !isLast && !dontInterrupt && !advanceDialogueManually;
            }

        }
    }

    private void SetConditionalNames()
    {
        for (int i = 0; i < _target.Conds.Count; i++)
        {
            NPCConditionals npcConditional = _target.Conds[i];
            if (npcConditional.dialogueChain.Count > 0)
                npcConditional.name = $"{i}. {npcConditional.dialogueChain[0].dialogue}";
        }
    }
}

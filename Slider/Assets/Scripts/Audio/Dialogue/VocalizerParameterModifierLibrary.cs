using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Key = NPCEmotes.Emotes;

namespace SliderVocalization
{
    [CreateAssetMenu(menuName = "Scriptable Objects/VocalizerParameterModifierLibrary")]
    public class VocalizerParameterModifierLibrary : ScriptableObject
    {
        [SerializeField]
        private ModifierWrapper[] modifierList;

        public VocalizerParametersModifier this[Key t]
        {
            get
            {


                if (modifiers == null)
                {
                    modifiers = new Dictionary<Key, VocalizerParametersModifier>();
                    foreach (ModifierWrapper kv in modifierList)
                    {
                        modifiers[kv.type] = kv.value;
                    }
                }

#if UNITY_EDITOR
                // check whether all values are valid, obviously very redundant but isn't called frequently anyway (once per paragraph)
                foreach (Key type in (Key[])System.Enum.GetValues(typeof(Key)))
                {
                    var temp = modifiers[type]; // throws error when not accessible
                }
#endif

                return modifiers[t];
            }
        }
        private Dictionary<Key, VocalizerParametersModifier> modifiers;

        [System.Serializable]
        public struct ModifierWrapper
        {
            public Key type;
            public VocalizerParametersModifier value;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(VocalizerParameterModifierLibrary))]
    public class VocalizerParameterModifierLibraryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("RUNTIME EDIT DOES NOT REFLECT IN GAME");
                GUI.enabled = false;
            }
            if (GUILayout.Button("Check"))
            {
                VocalizerParameterModifierLibrary lib = (VocalizerParameterModifierLibrary)target;
                foreach (Key type in (Key[])System.Enum.GetValues(typeof(Key)))
                {
                    var temp = lib[type]; // throws error when not accessible
                }
            }
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
#endif
}
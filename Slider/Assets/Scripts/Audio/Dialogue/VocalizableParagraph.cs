using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SliderVocalization
{
    public class VocalizableParagraph : MonoBehaviour, IVocalizerComposite<SentenceVocalizer>
    {
        private const string DEFAULT_VOCALIZER_PRESET_NAME = "Default Vocalizer Preset";

        public VocalizerPreset preset;
        [SerializeField, HideInInspector] private string text;
        [SerializeField, HideInInspector] private List<SentenceVocalizer> sentences;
        public VocalizationContext currentVocalizationContext;
        [SerializeField] private VocalizerParameterModifierLibrary modifierLibrary;

        public string Text => text;
        public List<SentenceVocalizer> Vocalizers => sentences;

        private SentenceVocalizer _Current;
        private VocalizerCompositeState _state;

        private void Awake() 
        {
            if (name == "Vocalizer" && preset != null && preset.name == DEFAULT_VOCALIZER_PRESET_NAME && transform.parent != null)
            {
                Debug.LogWarning($"{transform.parent.name} is using the default localizer!");
            }
        }

        public static bool SoloSpeaker(VocalizableParagraph target, int maxConcurrent)
        {
            if (speakers.Count <= maxConcurrent)
            {
                return true;
            }

            for (int i = speakers.Count - maxConcurrent; i < speakers.Count; i++)
            {
                if (speakers[i] == target)
                {
                    return true;
                }
            }

            return false;
        }
        internal static List<VocalizableParagraph> speakers = new();

        internal void StartReadSentence_Debug(SentenceVocalizer voc, NPCEmotes.Emotes emote)
        {
            this.Stop();
            voc.Stop();

            currentVocalizationContext = new(transform, this);
            StartCoroutine(voc.Vocalize(((VocalizerParameters) preset).ModifyWith(modifierLibrary[emote], createClone: true), currentVocalizationContext));
        }

        public void StartReadAll(NPCEmotes.Emotes emote)
        {
            currentVocalizationContext = new(transform, this);
            
            this.Stop(); // AT: removes self from speakers! must call before adding back into speakers
            
            // Technically this is done within the coroutine, but I'm not sure if there is guarantee that the coroutine will evaluate on the first frame
            // Setting this to playing will *guarantee* no multi-start issues
            this.MarkAsStarted();
            
            // Debug.Log($"Speaker registered at {transform.parent.name}");
            speakers.Add(this);
            
            StartCoroutine(
                this.Vocalize(
                    ((VocalizerParameters)preset).ModifyWith(modifierLibrary[emote], createClone: true), 
                    currentVocalizationContext)
                );
        }

        /// <summary>
        /// Parses text and generates randomized narration
        /// </summary>
        /// <param name="text">Duration of that narration when played uninterrupted</param>
        /// <returns></returns>
        public float SetText(string text, NPCEmotes.Emotes emote, float textSpeedMultiplier, int maxPhonemes = int.MaxValue)
        {
            this.text = text;
            sentences = SentenceVocalizer.Parse(this.text, preset, maxPhonemes) ?? new();
            
            return (this as IVocalizer).RandomizeVocalization(
                ((VocalizerParameters)preset).ModifyWith(modifierLibrary[emote], createClone: true),
                new VocalRandomizationContext
                {
                    textSpeedMultiplier = textSpeedMultiplier,
                }
                );
        } 

        void IVocalizerComposite<SentenceVocalizer>.PreRandomize(
            VocalizerParameters preset, VocalRandomizationContext context, SentenceVocalizer upcoming) { }

        public VocalizerCompositeState GetVocalizationState()
        {
            return _state;
        }

        void IVocalizerComposite<SentenceVocalizer>.SetVocalizationState(VocalizerCompositeState newState)
        {
            _state = newState;
        }

        public override string ToString()
        {
            return Text;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(VocalizableParagraph))]
    public class VocalizerDebuggerEditor : Editor
    {
        string rawText;
        NPCEmotes.Emotes emote;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var reader = target as VocalizableParagraph;

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField(reader.GetVocalizationState().ToString());

                GUIStyle textAreaStyle = new(EditorStyles.textArea)
                {
                    wordWrap = true
                };
                rawText = EditorGUILayout.TextArea(rawText, textAreaStyle, GUILayout.MinHeight(100), GUILayout.ExpandHeight(true));

                emote = (NPCEmotes.Emotes) EditorGUILayout.EnumFlagsField(emote);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply"))
                {
                    reader.SetText(rawText, emote, textSpeedMultiplier: 1.0f);
                    EditorUtility.SetDirty(reader);
                }
                if (reader.Vocalizers != null && reader.Vocalizers.Count > 0 && Application.isPlaying)
                {
                    if (GUILayout.Button("Play"))
                    {
                        reader.StartReadAll(emote);
                    }
                }
                if (reader.GetVocalizationState() == VocalizerCompositeState.Playing)
                {
                    if (GUILayout.Button("Stop"))
                    {
                        reader.Stop();
                    }
                }
                EditorGUILayout.EndHorizontal();

                GUIStyle vocalizerPreviewStyle = new(GUI.skin.label)
                {
                    richText = true
                };

                if (reader.Vocalizers != null)
                {
                    foreach (var v in reader.Vocalizers)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(v.punctuation.ToString());
                        if (Application.isPlaying && GUILayout.Button("Read clause"))
                        {
                            reader.StartReadSentence_Debug(v, emote);
                        }
                        EditorGUILayout.EndHorizontal();
                        string sentence = "";
                        foreach (var w in v.words)
                        {
                            sentence += w.ToString() + " ";
                        }
                        EditorGUILayout.LabelField(sentence, vocalizerPreviewStyle);
                    }
                }

                // this is to keep the playback up to date
                // sometimes Unity skips repainting the inspector so playback progress lags
                Repaint();
            }
            else
            {
                EditorGUILayout.LabelField("ONLY EDITABLE LIVE DURING PLAY");
            }
        }
    }

#endif
}
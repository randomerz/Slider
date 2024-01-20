using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(AIBrain), true)]
	public class AIBrainEditor : Editor
	{
		protected MMReorderableList _list;
		protected SerializedProperty _brainActive;
		protected SerializedProperty _resetBrainOnEnable;
		protected SerializedProperty _resetBrainOnStart;
		protected SerializedProperty _timeInThisState;
		protected SerializedProperty _target;
		protected SerializedProperty _owner;
		protected SerializedProperty _actionsFrequency;
		protected SerializedProperty _decisionFrequency;
		protected SerializedProperty _randomizeFrequencies;
		protected SerializedProperty _randomActionFrequency;
		protected SerializedProperty _randomDecisionFrequency;

		protected virtual void OnEnable()
		{
			_list = new MMReorderableList(serializedObject.FindProperty("States"));
			_list.elementNameProperty = "States";
			_list.elementDisplayType = MMReorderableList.ElementDisplayType.Expandable;

			_brainActive = serializedObject.FindProperty("BrainActive");
			_resetBrainOnEnable = serializedObject.FindProperty("ResetBrainOnEnable");
			_resetBrainOnStart = serializedObject.FindProperty("ResetBrainOnStart");
			_timeInThisState = serializedObject.FindProperty("TimeInThisState");
			_target = serializedObject.FindProperty("Target");
			_owner = serializedObject.FindProperty("Owner");
			_actionsFrequency = serializedObject.FindProperty("ActionsFrequency");
			_decisionFrequency = serializedObject.FindProperty("DecisionFrequency");
            
			_randomizeFrequencies = serializedObject.FindProperty("RandomizeFrequencies");
			_randomActionFrequency = serializedObject.FindProperty("RandomActionFrequency");
			_randomDecisionFrequency = serializedObject.FindProperty("RandomDecisionFrequency");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			_list.DoLayoutList();
			EditorGUILayout.PropertyField(_timeInThisState);
			EditorGUILayout.PropertyField(_owner);
			EditorGUILayout.PropertyField(_target);
			EditorGUILayout.PropertyField(_brainActive);
			EditorGUILayout.PropertyField(_resetBrainOnEnable);
			EditorGUILayout.PropertyField(_resetBrainOnStart);
			EditorGUILayout.PropertyField(_actionsFrequency);
			EditorGUILayout.PropertyField(_decisionFrequency);
			EditorGUILayout.PropertyField(_randomizeFrequencies);
			if (_randomizeFrequencies.boolValue)
			{
				EditorGUILayout.PropertyField(_randomActionFrequency);
				EditorGUILayout.PropertyField(_randomDecisionFrequency);
			}
			serializedObject.ApplyModifiedProperties();

			AIBrain brain = (AIBrain)target;
			if (brain.CurrentState != null)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Current State", brain.CurrentState.StateName);
			}
		}
	}
}
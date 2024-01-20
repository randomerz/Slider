using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

namespace MoreMountains.Tools
{
	[CanEditMultipleObjects()]
	[CustomEditor(typeof(MMRendererSortingLayer), true)]
	public class MMRendererLayerEditor : Editor
	{
		int popupMenuIndex;
		string[] sortingLayerNames;
		protected MMRendererSortingLayer _mmRendererSortingLayer;
		protected Renderer _renderer;

		void OnEnable()
		{
			sortingLayerNames = GetSortingLayerNames(); 
			_mmRendererSortingLayer = (MMRendererSortingLayer)target;
			_renderer = _mmRendererSortingLayer.GetComponent<Renderer> ();

			for (int i = 0; i<sortingLayerNames.Length;i++) //here we initialize our popupMenuIndex with the current Sort Layer Name
			{
				if (sortingLayerNames[i] == _renderer.sortingLayerName)
					popupMenuIndex = i;
			}
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (_renderer == null)
			{
				return; 
			}

			popupMenuIndex = EditorGUILayout.Popup("Sorting Layer", popupMenuIndex, sortingLayerNames);
			int newSortingLayerOrder = EditorGUILayout.IntField("Order in Layer", _renderer.sortingOrder);
		
			if (sortingLayerNames[popupMenuIndex] != _renderer.sortingLayerName 
			    || newSortingLayerOrder != _renderer.sortingOrder) 
			{
				Undo.RecordObject(_renderer, "Change Particle System Renderer Order"); 

				_renderer.sortingLayerName = sortingLayerNames[popupMenuIndex];
				_renderer.sortingOrder = newSortingLayerOrder;

				EditorUtility.SetDirty(_renderer); 
			}
		}

		public string[] GetSortingLayerNames()
		{
			Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])sortingLayersProperty.GetValue(null, new object[0]);
		}
	}
}
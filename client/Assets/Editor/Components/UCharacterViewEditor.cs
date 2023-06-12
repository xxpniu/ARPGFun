using UnityEngine;
using System.Collections;
using BattleViews;
using BattleViews.Views;
using UnityEditor;
using GameLogic.Game.Elements;

[CustomEditor(typeof(UCharacterView))]
public class UCharacterViewEditor : Editor
{
	private bool showProperties = false;

	public override void OnInspectorGUI()
	{
		EditorGUILayout.BeginVertical();
		var uCharacterView = this.target as UCharacterView;
		if (GUILayout.Button("View AI Tree"))
		{
			
			var window = EditorWindow.GetWindow<AITreeEditor>();
			if (window == null) return;
			if (uCharacterView!.GElement is not BattleCharacter character) return;
			var root = character.AiRoot;
			if (root == null)
			{
				EditorUtility.DisplayDialog("Failure", "Current character no ai tree", "OK");
			}
			else
			{
				root.IsDebug = true;
				window.AttachRoot(root);
				AIRunner.Current?.Attach(character);
			}
		}
		if (GUILayout.Button("Kill"))
		{
			if (uCharacterView!.GElement is not BattleCharacter character) return;
			character.SubHP(character.MaxHP,out _);
		}

		showProperties = EditorGUILayout.Toggle("Display Properties", showProperties);
		if (showProperties)
		{
			foreach (var i in uCharacterView!.properties)
			{
				EditorGUILayout.LabelField($"[{(int)i.Property}]{i.Property}:{i.Value}");
			}
		}

		EditorGUILayout.EndVertical();
		base.OnInspectorGUI();
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(GameManager))]
public class ManagerEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();
        
		GameManager maze = (GameManager)target;
		if (GUILayout.Button ("Set Up Game")) {
//			maze.SetUpGame ();
		}
	}
}

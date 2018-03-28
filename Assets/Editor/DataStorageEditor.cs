using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

[CustomEditor(typeof(DataStorage))]
public class DataStorageEditor : Editor {

	string StatusMessage = "";

	public override void OnInspectorGUI ()
	{
		DataStorage Script = (DataStorage)target;
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Version Number:");
        for (int i = 0; i < 3; i++)
        {
            Script.Version[i] = GUILayout.TextField(Script.Version[i]);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Upload Result Text:");
        Script.uploadResultText = (ResultText)EditorGUILayout.ObjectField(Script.uploadResultText, typeof(ResultText), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Download Result Text:");
        Script.downloadResultText = (ResultText)EditorGUILayout.ObjectField(Script.downloadResultText, typeof(ResultText), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Save Button:");
        Script.saveButton = (Button)EditorGUILayout.ObjectField(Script.saveButton, typeof(Button), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Before Match Button:");
        Script.beforeMatchButton = (Button)EditorGUILayout.ObjectField(Script.beforeMatchButton, typeof(Button), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal ("box");
		GUILayout.Label ("Base Server URL:");
		Script.serverBaseURL = GUILayout.TextField (Script.serverBaseURL);
		GUILayout.EndHorizontal ();
		if (Script.data.Count <= 0) {
			GUILayout.Label ("Currenty no data to display!");
			return;
		}
		foreach (KeyValuePair<string,string> kvp in Script.data) {
			GUILayout.BeginHorizontal ("box");
			GUILayout.Label (kvp.Key);
			GUILayout.Label (kvp.Value);
			GUILayout.EndHorizontal ();
		}
		if (GUILayout.Button ("Click to save to file and clear!")) {
			StatusMessage = "Data saved to: " + Script.saveToFile (true);
		}
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Click to Download."))
        {
            Script.StartCoroutine(Script.downloadJson());
        }
        if (GUILayout.Button("Click to Sync."))
        {
            Script.sync();
        }
        if (GUILayout.Button("Click to Upload."))
        {
            Script.StartCoroutine(Script.uploadData());
        }
        GUILayout.EndHorizontal();
        GUILayout.Label (StatusMessage);
	}
}

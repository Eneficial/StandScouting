using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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
        GUILayout.Label("Result Text:");
        Script.uploadResultText = (ResultText)EditorGUILayout.ObjectField(Script.uploadResultText, typeof(ResultText), true);
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
        if (GUILayout.Button("Click to download JSON."))
        {
            Script.StartCoroutine(Script.downloadJSON());
        }
		if (GUILayout.Button ("Click to upload data!")) {
			Script.uploadData();
			StatusMessage = "Data Uploading...";
		}
		GUILayout.Label (StatusMessage);
	}
}

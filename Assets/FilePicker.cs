using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;

public class FilePicker : MonoBehaviour {
	public DataStorage DS;
	public GameObject MainMenu;
	public GameObject ScoutingMenu;
	public GameObject FileMenu;
	public Dropdown dropdown;
	public Text dataViewer;
	public FileInfo file;
	public Text resultText;

	public void loadMenu () {
		FileMenu.SetActive (true);
		MainMenu.SetActive (true);
		ScoutingMenu.SetActive (true);
	}

	public void Start() {
		DS = GetComponent<DataStorage>();
	}

	public void LateUpdate() {
		if (dropdown == null)
			return;
		if (dropdown.value == 0 || dropdown.options[dropdown.value].text == ".DS_Store")
			return;
		FileInfo tmp_file = new FileInfo (Application.persistentDataPath + Path.DirectorySeparatorChar + dropdown.captionText.text);
        if (file == null)
        {
            if (tmp_file.Exists)
            {
                file = tmp_file;
                dataViewer.text = getStringFromFile(file);
            }
        }
        if (file != null && !file.Exists)
        {
            dropdown.value = 0;
            dropdown.RefreshShownValue();
            return;
        }
		if (tmp_file.Equals(file))
			return;
		file = tmp_file;
		dataViewer.text = getStringFromFile (file);
	}

	public void Update() {
		if (dropdown == null)
			return;
		List<Dropdown.OptionData> files = new List<Dropdown.OptionData>();
		DirectoryInfo directory = new DirectoryInfo (Application.persistentDataPath);
		files.Add (new Dropdown.OptionData("Choose a file"));
		foreach (FileInfo file in directory.GetFiles()) {
			if (file.Name.StartsWith (".") || file.Extension != ".txt")
				continue;
			files.Add (new Dropdown.OptionData (file.Name));
		}
		if (dropdown.options != files) {
			dropdown.options = files;
			dropdown.RefreshShownValue ();
		}
	}

	public string getStringFromFile(FileInfo file) {
        if (!file.Exists) return "File not found.";
		StreamReader sr = file.OpenText();
		string data = sr.ReadToEnd ();
		sr.Close ();
		return data;
	}

	public void loadFromSelectedFile() {
		if (file == null)
			return;
		string raw = getStringFromFile (file);
		bool valid = false;
		foreach (string line in raw.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)) {
			if (line.Split (';') [0] == "Version")
				valid = true;
		}
		if (!valid) {
			resultText.text = "Error: File is not valid, missing Version tag.";
            Debug.LogWarning("Error: File is not valid, missing Version tag. Raw data: " + raw);
			return;
		}
		foreach (string line in raw.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)) {
			if (line == null)
				continue;
			string[] broken = line.Split (';');
			if (broken [0].Equals ("Version") && broken [1] != DS.data ["Version"]) {
				resultText.text = "Error: Version mismatch between " + DS.data ["Version"] + " and " + broken [1];
				return;
			}
			Debug.Log ("Attempting to access at key " + broken [0]);
			if (!DS.inputs.ContainsKey(broken[0]))
				continue;
			DS.inputs[broken[0]].changeData(broken[1]);
			resultText.text = "The file has been loaded!";
		}
	}

    public void clearFiles()
    {
        DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath);
        foreach (FileInfo file in directory.GetFiles())
        {
            if (file.Extension == ".txt")
            {
                resultText.text = "Deleting file " + file.Name;
                file.Delete();
                resultText.text = "Deleted file " + file.Name;
            }
        }
        resultText.text = "Finished deleting unuploaded files. Clearing uploaded files.";
        DirectoryInfo uploaded = new DirectoryInfo(Application.persistentDataPath + Path.DirectorySeparatorChar + "uploaded");
        if (uploaded.Exists)
        {
            foreach (DirectoryInfo folder in uploaded.GetDirectories())
            {
                if (folder.GetFiles().Length > 0)
                {
                    foreach (FileInfo file in folder.GetFiles())
                    {
                        resultText.text = "Deleting file " + file.Name + " in uploaded" + Path.DirectorySeparatorChar + folder.Name;
                        file.Delete();
                        resultText.text = "Deleted file " + file.Name + " in uploaded" + Path.DirectorySeparatorChar + folder.Name;
                    }
                }
                resultText.text = "Deleting folder for team " + folder.Name;
                folder.Delete();
                resultText.text = "Deleted folder for team " + folder.Name;
            }
            resultText.text = "Deleting upload folder.";
            uploaded.Delete();
            resultText.text = "Deleted upload folder.";
        }
        resultText.text = "Clearing done.";
    }
}

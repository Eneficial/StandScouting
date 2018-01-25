using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.Networking;
using System.Collections.Specialized;
using System.Net;
using System.Text;

public class DataStorage : MonoBehaviour {
	public Dictionary<string,string> data = new Dictionary<string, string>();
	public Dictionary<string,DataInput> inputs = new Dictionary<string, DataInput> ();
	public string serverBaseURL = "";
    public string[] Version = { "2017", "1", "0" };

    private WebClient client = new WebClient();

    private UnityWebRequest currentDownloadRequest;
    private WWW currentUploadRequest;
    public ResultText uploadResultText;
    public ResultText downloadResultText;
	// Use this for initialization
	void Start () {
		//data = new Dictionary<string, string>();

		//Setup static values
		data.Add("Version", "v"+Version[0]+"."+ Version[1] + "." + Version[2]);
        data.Add("ScouterName", "");
	}

    void LateUpdate()
    {
        if (currentDownloadRequest != null)
        {
            float progress = (currentDownloadRequest.uploadProgress + currentDownloadRequest.downloadProgress) / 2;
            downloadResultText.setText("Download progress: " + progress.ToString("P2"));

        }
        if (currentUploadRequest != null)
        {
            float progress = (currentUploadRequest.uploadProgress + currentUploadRequest.progress) / 2;
            uploadResultText.setText("Upload progress: " + progress.ToString("P2"));

        }
    }

    public bool addData(string key, string value, bool overwrite) {
		try {
			data.Add (key, value);
			return true;
		} catch (ArgumentNullException e) {
			return false;
		} catch (ArgumentException	e) {
			if (!overwrite) {
				return false;
			}
			data [key] = value;
			return true;
		}
	}

	public bool addData(string key, string value, bool overwrite, DataInput DI) {
		try {
			inputs.Add(key,DI);
			Debug.Log("Added input at key " + key); 
		} catch (ArgumentNullException e) {
			return false;
		} catch (ArgumentException e) {
			if (!overwrite) {
				return false;
			}
			inputs [key] = DI;
		}
		return addData (key, value, overwrite);
	}

	/** 
	*Saves the current data to the file system.
	*If clear is true, clears the data from the DataStorage after saving it.
	**/
	public string saveToFile() {
		return saveToFile (false);
	}

	public void saveAndClear() {
		saveToFile (true);
	}

	/** 
	*Saves the current data to the file system.
	*If clear is true, clears the data from the DataStorage after saving it.
	**/
	public string saveToFile(bool clear) {
		string filePath = Application.persistentDataPath + Path.DirectorySeparatorChar + data ["TeamNumber"] + ".txt"; //Default path, may need adjusting if duplicate fils
		if (File.Exists (filePath)) {
			int maxCount = 1000;
			for (int i = 1; i < maxCount+1; i++) {
				string tmpFilePath = Application.persistentDataPath + Path.DirectorySeparatorChar + data ["TeamNumber"] + "-" + (maxCount - i+1) + ".txt"; //Try different possibilities until 1000, then give up
				if (!File.Exists (tmpFilePath)) {
					filePath = tmpFilePath;
				}
				if (i == maxCount && filePath == Application.persistentDataPath + Path.DirectorySeparatorChar + data ["TeamNumber"] + ".txt") {
					Debug.LogError ("Too many files! Couldn't save data, not clearing");
					return "error";
				}
			}
		}
		using (StreamWriter sw = File.CreateText (filePath)) {
			foreach (KeyValuePair<string,string> kvp in data) {
				if (inputs.ContainsKey (kvp.Key) && clear && !isStaticKey(kvp.Key))
					inputs [kvp.Key].clearData();
				sw.WriteLine (kvp.Key + ";" + kvp.Value.Replace(';',':'));
			}
		}
		return filePath;
	}

    private bool isStaticKey(string key)
    {
        return key == "ScouterName" || key == "Version" || key == "EventKey";
    }

    public void sync() {
        StartCoroutine(downloadJson());
        StartCoroutine(uploadData());
    }

    public IEnumerator downloadJson() {

        Debug.Log("Starting download of JSON");
        UnityWebRequest wwwDownloadRequest = UnityWebRequest.Get(serverBaseURL + "/api/v1/syncDownload.php");
        currentDownloadRequest = wwwDownloadRequest;
        yield return wwwDownloadRequest.SendWebRequest();
        currentDownloadRequest = null;

        Debug.Log("Download Completed.");
        if (!wwwDownloadRequest.isHttpError)
        {
            SyncData data = JsonUtility.FromJson<SyncData>(wwwDownloadRequest.downloadHandler.text);
            GetComponent<EventTeamData>().clearData();
            GetComponent<EventTeamData>().loadData(data);

            if (File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + "data.json"))
                File.Delete(Application.persistentDataPath + Path.DirectorySeparatorChar + "data.json");

            StreamWriter sw = File.CreateText(Application.persistentDataPath + Path.DirectorySeparatorChar + "data.json");
            sw.Write(wwwDownloadRequest.downloadHandler.text);
            sw.Close();

            if (data.CurrentVersion[2] != Version[2])
            {
                Debug.Log("Revision Version mismatch.");
            }
            if (data.CurrentVersion[1] != Version[1])
            {
                Debug.LogWarning("Minor Version mismatch.");
                uploadResultText.setText("Warning: Minorly out of date. Please update if possible.");
            }
            if (data.CurrentVersion[0] != Version[0])
            {
                Debug.LogWarning("Major/Year Version mismatch.");
                uploadResultText.setText("Warning: Majorly out of date. Please update ASAP.");
            }
        } else
        {
            downloadResultText.setText("Error encountered downloading from server.");
            Debug.LogError("Code: " + wwwDownloadRequest.responseCode + " Error: " + wwwDownloadRequest.error);
        }
        wwwDownloadRequest.Dispose();
    }
    public IEnumerator uploadData() {
		DirectoryInfo dinfo = new DirectoryInfo(Application.persistentDataPath);
        
		foreach (FileInfo file in dinfo.GetFiles()) {
			if (file.Name.StartsWith (".") || !file.Extension.Equals(".txt"))
				continue;

            WWWForm formData = new WWWForm();
            formData.AddField("App","stand");
            // Open the stream and read it back.
            using (StreamReader sr = file.OpenText ()) {
				string s = "";
				while ((s = sr.ReadLine ()) != null) {
					string[] data = s.Split (';');

					Debug.Log (file.Name + ":" + data [0] + " " + data [1]);
                    uploadResultText.setText(file.Name + ":" + data[0] + " " + data[1]);
                    formData.AddField(data[0],data[1]);
                }
			}
            Debug.Log("Creating request");
            WWW uploadRequest = new WWW(serverBaseURL + "/api/v1/submit.php", formData);
            Debug.Log("Form upload begun for file " + file.Name);
            currentUploadRequest = uploadRequest;
            Debug.Log(uploadRequest.url);
            yield return uploadRequest;
            if (uploadRequest.error != null)
            {
                string path = Application.persistentDataPath + Path.DirectorySeparatorChar + "uploaded" + Path.DirectorySeparatorChar + file.Name.Split('-')[0].Split('.')[0];
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (!File.Exists(path + Path.DirectorySeparatorChar + file.Name))
                    file.MoveTo(path + Path.DirectorySeparatorChar + file.Name);
            } else
            {
                uploadResultText.setText("An error has occured.");
                Debug.LogError("Error uploading file " + file.Name + " Error Code: " + uploadRequest.error);
                continue;
            }

            
            Debug.Log("Request complete.");

            

            yield return new WaitForSeconds(0.25f);
        }
	}
}


[System.Serializable]
public class SyncData
{
    public string[] CurrentVersion;
    public EventData[] Events;
    public EventTeamList[] TeamsByEvent;
    public MatchSync[] EventMatches;
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DataStorage : MonoBehaviour
{
    public Dictionary<string, string> data = new Dictionary<string, string>();
    public Dictionary<string, DataInput> inputs = new Dictionary<string, DataInput>();
    public string serverBaseURL = "";
    public string[] Version = { "2017", "1", "0" };
    public Button saveButton;
    public Button beforeMatchButton;

    private UnityWebRequest currentDownloadRequest;
    private UnityWebRequest currentUploadRequest;
    public ResultText uploadResultText;
    public ResultText downloadResultText;
    // Use this for initialization
    void Start()
    {
        //data = new Dictionary<string, string>();

        //Setup static values
        data.Add("Version", "v" + Version[0] + "." + Version[1] + "." + Version[2]);
        data.Add("ScouterName", "");
    }

    void LateUpdate()
    {
        if (currentDownloadRequest != null)
        {
            downloadResultText.setText("Download JSON progress: " + currentDownloadRequest.downloadProgress.ToString("P2"));

        }
        if (currentUploadRequest != null)
        {
            //float progress = (currentUploadRequest.uploadProgress + currentUploadRequest.progress) / 2;
            uploadResultText.setText("Upload data progress: " + currentUploadRequest.uploadProgress.ToString("P2"));

        }
    }

    public bool addData(string key, string value, bool overwrite)
    {
        try
        {
            data.Add(key, value);
            return true;
        }
        catch (ArgumentNullException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            if (!overwrite)
            {
                return false;
            }
            data[key] = value;
            return true;
        }
    }

    public bool addData(string key, string value, bool overwrite, DataInput DI)
    {
        try
        {
            inputs.Add(key, DI);
            Debug.Log("Added input at key " + key);
        }
        catch (ArgumentNullException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            if (!overwrite)
            {
                return false;
            }
            inputs[key] = DI;
        }
        return addData(key, value, overwrite);
    }

    /** 
	*Saves the current data to the file system.
	*If clear is true, clears the data from the DataStorage after saving it.
	**/
    public string saveToFile()
    {
        return saveToFile(false);
    }

    public void saveAndClear()
    {
        saveButton.interactable = false;
        saveToFile(true);
        saveButton.interactable = true;
        beforeMatchButton.onClick.Invoke();
    }

    /** 
	*Saves the current data to the file system.
	*If clear is true, clears the data from the DataStorage after saving it.
	**/
    public string saveToFile(bool clear)
    {
        string filePath = Application.persistentDataPath + Path.DirectorySeparatorChar + data["TeamNumber"] + ".txt"; //Default path, may need adjusting if duplicate fils
        if (File.Exists(filePath))
        {
            int maxCount = 1000;
            for (int i = 1; i < maxCount + 1; i++)
            {
                string tmpFilePath = Application.persistentDataPath + Path.DirectorySeparatorChar + data["TeamNumber"] + "-" + (maxCount - i + 1) + ".txt"; //Try different possibilities until 1000, then give up
                if (!File.Exists(tmpFilePath))
                {
                    filePath = tmpFilePath;
                }
                if (i == maxCount && filePath == Application.persistentDataPath + Path.DirectorySeparatorChar + data["TeamNumber"] + ".txt")
                {
                    Debug.LogError("Too many files! Couldn't save data, not clearing");
                    return "error";
                }
            }
        }
        using (StreamWriter sw = File.CreateText(filePath))
        {
            foreach (KeyValuePair<string, string> kvp in data)
            {
                if (inputs.ContainsKey(kvp.Key) && clear && !isStaticKey(kvp.Key))
                    inputs[kvp.Key].clearData();
                sw.WriteLine(kvp.Key + ";" + kvp.Value.Replace(';', ':'));
            }
        }
        return filePath;
    }

    private bool isStaticKey(string key)
    {
        return key == "ScouterName" || key == "ScouterTeamNumber" || key == "Version" || key == "EventKey";
    }

    public void sync()
    {
        StartCoroutine(syncRoutine());
    }

    private bool lastPingTest = false;
    public IEnumerator syncRoutine()
    {
        yield return pingTest();

        if (lastPingTest)
        {
            StartCoroutine(downloadJson());
            StartCoroutine(uploadData());
        } else
        {
            downloadResultText.setText("Failed to connect to server,");
            uploadResultText.setText("please try again later.");
        }
    }

    private IEnumerator pingTest()
    {
        UnityWebRequest pingWebRequest = UnityWebRequest.Get(serverBaseURL + "/api/v1/ping.php");
        pingWebRequest.timeout = 5;
        yield return pingWebRequest.SendWebRequest();
        string text = pingWebRequest.downloadHandler.text;
        Debug.Log(text);
        pingWebRequest.Dispose();
        lastPingTest = text == "pong";
    }

    public IEnumerator downloadJson()
    {
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
                downloadResultText.setText("Warning: Minorly out of date. Please update if possible.");
            }
            if (data.CurrentVersion[0] != Version[0])
            {
                Debug.LogWarning("Major/Year Version mismatch.");
                downloadResultText.setText("Warning: Majorly out of date. Please update ASAP.");
            }
            else
            {
                downloadResultText.setText("Download complete.");
            }
        }
        else
        {
            downloadResultText.setText("Error encountered downloading from server.");
            Debug.LogError("Code: " + wwwDownloadRequest.responseCode + " Error: " + wwwDownloadRequest.error);
        }
        wwwDownloadRequest.Dispose();
    }
    public IEnumerator uploadData()
    {
        DirectoryInfo dinfo = new DirectoryInfo(Application.persistentDataPath);

        foreach (FileInfo file in dinfo.GetFiles())
        {
            if (file.Name.StartsWith(".") || !file.Extension.Equals(".txt"))
                continue;

            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData["App"] = "stand";
            // Open the stream and read it back.
            using (StreamReader sr = file.OpenText())
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    string[] data = s.Split(';');

                    Debug.Log(file.Name + ":" + data[0] + " " + data[1]);
                    uploadResultText.setText(file.Name + ":" + data[0] + " " + data[1]);
                    if (data[1] == "") data[1] = " ";
                    formData[data[0]] = data[1];
                }
            }
            Debug.Log("Creating request");
            UnityWebRequest uploadRequest = UnityWebRequest.Post(serverBaseURL + "/api/v1/submit.php", formData);
            Debug.Log("Form upload begun for file " + file.Name);
            currentUploadRequest = uploadRequest;
            uploadRequest.chunkedTransfer = false;
            yield return uploadRequest.SendWebRequest();
            Debug.Log(uploadRequest.downloadHandler.text);
            if (!uploadRequest.isHttpError && JsonUtility.FromJson<ValidatorData>(uploadRequest.downloadHandler.text).App == formData["App"])
            {
                file.Delete();
            }
            else
            {
                uploadResultText.setText("An error has occured.");
                Debug.LogError("Error uploading file " + file.Name + " Error Code: " + uploadRequest.responseCode);
                continue;
            }


            Debug.Log("Request complete.");



            yield return new WaitForSeconds(0.25f);
        }
        currentUploadRequest = null;
        uploadResultText.setText("Uploading Complete");
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

[System.Serializable]
public class ValidatorData //Used http://json2csharp.com/ to generate this.
{
    public string App { get; set; }
    public string Version { get; set; }
    public string ScouterName { get; set; }
    public string ScouterTeamNumber { get; set; }
    public string EventKey { get; set; }
    public string TeamNumber { get; set; }
    public string NoAlliance { get; set; }
    public string Pre_StartingPos { get; set; }
    public string Auto_CrossedBaseline { get; set; }
    public string Auto_Notes { get; set; }
    public string Auto_PlaceSwitch { get; set; }
    public string Auto_PlaceScale { get; set; }
    public string Teleop_ScalePlace { get; set; }
    public string Teleop_SwitchPlace { get; set; }
    public string Teleop_ExchangeVisit { get; set; }
    public string Teleop_Notes { get; set; }
    public string Notes { get; set; }
    public string Pre_NoShow { get; set; }
    public string MatchNumber { get; set; }
    public string Teleop_BoostUsed { get; set; }
    public string Teleop_ForceUsed { get; set; }
    public string Teleop_LevitateUsed { get; set; }
    public string Post_Climb { get; set; }
    public string DOF { get; set; }
}
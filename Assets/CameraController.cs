using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class CameraController : MonoBehaviour {

    public WebCamTexture cameraTexture = null;
    public DataStorage ds;
    public GameObject snapButton;
    public GameObject resnapButton;
    public string cameraName = "";
    public bool setup = false;

    // Use this for initialization
    void Start()
    {
        if (WebCamTexture.devices.Length < 1)
        {
            Debug.LogWarning("No cameras! No init.");
            return;
        }
        cameraTexture = new WebCamTexture();
        GetComponent<RawImage>().texture = cameraTexture;
        cameraName = cameraTexture.deviceName;
        setup = true;
        cameraTexture.Play();
    }

    // Not actually required, but auto adjusts the size of the viewfinder to 1/3 of the camera resolution
    void Update()
    {
        if (!setup)
            return;

        if (cameraTexture.width < 100)
            return;
        else
            GetComponent<RectTransform>().sizeDelta = new Vector2(cameraTexture.width / 3, cameraTexture.height / 3);
    }

    // Pauses the image to allow the user to retake or save a photo
    public void snap()
    {
        if (!setup)
            return;

        cameraTexture.Pause();
        snapButton.SetActive(false);
        resnapButton.SetActive(true);
    }

    // Resets the system so that another picture may be taken. Also used if wanting to retake a photo.
    public void reset()
    {
        if (!setup)
            return;
        cameraTexture.Stop();
        resnapButton.SetActive(false);
        snapButton.SetActive(true);
        cameraTexture.Play();
    }

    // Saves the photo to a persistant data folder provided by unity. Looks for a unique file name 1000 times, then gives up.
    public void saveToDisk()
    {
        if (!setup)
            return;

        Texture2D picture = new Texture2D(cameraTexture.width, cameraTexture.height); // Create a texture to recieve the image
        picture.SetPixels(cameraTexture.GetPixels()); // Copy the pixels from the WebCamTexture to the Texture2D

        string filePath = Application.persistentDataPath + Path.DirectorySeparatorChar + "robot-" + ds.data["TeamNumber"] + ".png"; // Default path, may need adjusting if duplicate fails
        if (File.Exists(filePath))
            File.Delete(filePath);

        File.WriteAllBytes(filePath, picture.EncodeToPNG()); // This is why I'm using Texture2D, that one function I needed to make this easy.
        reset();
    }
}

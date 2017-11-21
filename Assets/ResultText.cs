using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultText : MonoBehaviour {

    public void setText(string text)
    {
        GetComponent<Text>().text = text;
    }

    void OnDisable()
    {
        GetComponent<Text>().text = "";
    }
}

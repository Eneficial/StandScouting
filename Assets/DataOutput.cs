using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DataOutput : MonoBehaviour {
	public DataStorage DS;
	public string key;
	public string prefix;
	public string suffix;

	private Text text;

	void Start () {
		if (DS == null || key == null) this.enabled = false;
		text = GetComponent<Text> ();
	}
	
	void LateUpdate () {
		if (text != null) {
			if (DS.data.ContainsKey (key)) {
				text.text = prefix + DS.data [key] + suffix;
			} else {
				text.text = "ERROR: Key " + key + " doesn't exist!";
			}
		}
	}
}

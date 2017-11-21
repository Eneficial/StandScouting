using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DataCounter : DataInput {

    public DataStorage ds;
    public string prefix;
    public string suffix;
    Text text;
    int count = 0;

    // Use this for initialization
    void Start () {
        if (ds == null)
        {
            Debug.LogError(this.gameObject.name + " was unable to load because no DataStorage was attached!");
            this.gameObject.SetActive(false);
            return;
        }
        text = GetComponent<Text>();
	}

    void FixedUpdate()
    {
        ds.addData(this.gameObject.name, count.ToString(), true, this);
        text.text = prefix + count + suffix;
    }

	public override void changeData(object amountToChangeTo)
    {
		count = int.Parse(amountToChangeTo.ToString());
    }

    public void adjustCount(int amountToChangeBy)
    {
        count += amountToChangeBy;
    }
    
    public override void clearData()
    {
        count = 0;
    }
}

using UnityEngine;
using System.Collections;

public abstract class DataInput : MonoBehaviour {
	public abstract void changeData (object change);
    /*{
        Debug.Log("ERROR: You called DataInput directly with a value of " + change.ToString());
    }*/

	public abstract void clearData ();
	/*{
        Debug.Log("You can't clear what doesn't exist. You called DataInput directly. Don't do that. Object: "+gameObject.name);
    }
    */
}

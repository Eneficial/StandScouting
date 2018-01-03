using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventDropdown : DataInput {

    Dropdown dropdown;
    public DataStorage DS;
    public EventTeamData ETD;

	// Use this for initialization
	void Start () {
        dropdown = GetComponent<Dropdown>();
        refresh();
        dropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("ERR - Please Sync") });
        dropdown.RefreshShownValue();
        DS.addData("EventKey", "ERROR", true, this);
    }

    void LateUpdate()
    {
        if (dropdown.captionText.text == "ERR - Please Sync") return;
        DS.addData("EventKey", ETD.getSelectedEvent().key, true);
    }

    public void clear()
    {
        dropdown.ClearOptions();
        dropdown.RefreshShownValue();
    }

    public void refresh()
    {
        List<Dropdown.OptionData> tmpList = new List<Dropdown.OptionData>();
        foreach (EventData ed in ETD.getEvents())
        {
            tmpList.Add(new Dropdown.OptionData(ed.key + " - " + ed.name));
        }
        clear();
        dropdown.AddOptions(tmpList);
        dropdown.RefreshShownValue();
    }

    public override void changeData(object change)
    {
        if (ETD.getEvent(change.ToString()) != null)
        {
            EventData newEvent = ETD.getEvent(change.ToString());
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (ETD.getEvent(dropdown.options[i].text.Split(' ')[0]) == newEvent)
                {
                    dropdown.value = i;
                    break;
                }
            }
        }
    }

    public override void clearData()
    {
        refresh();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamDropdown : DataInput
{

    public Dropdown dropdown;
    public EventTeamData ETD;
    public DataStorage DS;

    // Use this for initialization
    void Start()
    {
        refresh();
        dropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("ERR - Please Sync") });
        dropdown.RefreshShownValue();
        DS.addData("TeamNumber", "0", true, this);
    }

    void LateUpdate()
    {
        if (dropdown.captionText.text == "ERR - Please Sync") return;
        string teamNumber = dropdown.captionText.text.Split(' ')[0];
        DS.addData("TeamNumber", teamNumber.Split(' ')[0], true);
    }

    public void clear()
    {
        dropdown.ClearOptions();
        dropdown.RefreshShownValue();
    }

    public void refresh()
    {
        List<Dropdown.OptionData> tmpList = new List<Dropdown.OptionData>();
        foreach (TeamInfo ti in ETD.getTeams(ETD.getSelectedEvent().key))
        {
            tmpList.Add(new Dropdown.OptionData(ti.team_number + " - " + ti.nickname));
        }
        clear();
        dropdown.AddOptions(tmpList);
        dropdown.RefreshShownValue();
    }

    public override void changeData(object change)
    {
        return;
    }

    public override void clearData()
    {
        refresh();
    }
}

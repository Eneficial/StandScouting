using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchDropdown : DataInput {

    public Dropdown dropdown;
    public DataStorage DS;
    public EventTeamData ETD;

    public bool setup = false;

	// Use this for initialization
	void Start () {
        //dropdown = GetComponent<Dropdown>();
        refresh();
        dropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("ERR - Please Sync") });
        dropdown.RefreshShownValue();
        DS.addData("MatchNumber", "-1", true, this);
    }

    void LateUpdate()
    {
        if (!setup) refresh();
        if (dropdown.captionText.text == "ERR - Please Sync") return;
        DS.addData("MatchNumber", dropdown.captionText.text, true);
    }

    public void clear()
    {
        dropdown.ClearOptions();
        dropdown.options = new List<Dropdown.OptionData>();
        dropdown.RefreshShownValue();
    }

    public void refresh()
    {
        if (ETD.getSelectedTeam() == null)
        {
            Debug.Log("1");
            return;
        }
        if (ETD.getSelectedEvent() == null)
        {
            Debug.Log("2");
            return;
        }
        if (ETD.getMatches(ETD.getSelectedTeam(), ETD.getSelectedEvent()) == null)
        {
            Debug.Log("3");
            return;
        }

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        foreach (int matchNumber in ETD.getMatches(ETD.getSelectedTeam(), ETD.getSelectedEvent()))
        {
            options.Add(new Dropdown.OptionData(matchNumber.ToString()));
        }
        clear();
        dropdown.options = options;
        dropdown.RefreshShownValue();
        setup = true;
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

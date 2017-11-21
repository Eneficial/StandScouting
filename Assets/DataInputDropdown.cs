using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataInputDropdown : DataInput {

    public DataStorage ds;
    public Dropdown dropdown;

    public override void changeData(object change)
    {
        if (change is string)
        {
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                Dropdown.OptionData option = dropdown.options[i];
                if (change.ToString() == option.text)
                {
                    dropdown.value = i;
                    dropdown.RefreshShownValue();
                    return;
                }
            }
        }
    }

    public override void clearData()
    {
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    void Start()
    {
        dropdown = GetComponent<Dropdown>();
    }

    void FixedUpdate()
    {
        ds.addData(this.gameObject.name, dropdown.captionText.text, true, this);
    }
}

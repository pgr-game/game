using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameDescription
{
    public string saveString;
    public string saveDate;

    public SaveGameDescription(string saveString, string saveDate) {
        this.saveString = saveString;
        this.saveDate = saveDate;
    }
}

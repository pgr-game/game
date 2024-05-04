using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameDescription
{
    public string saveString;
    public string mapName;
    public string saveDate;

    public SaveGameDescription(string saveString, string mapName, string saveDate) {
        this.saveString = saveString;
        this.mapName = mapName;
        this.saveDate = saveDate;
    }
}

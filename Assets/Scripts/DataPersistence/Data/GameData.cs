using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameData
{
    public long lastUpdated;
    public string playerInv;

    public int deathCount;
    //constructor appears to contain starting values for the data to be saved
    public GameData()
    {

        this.playerInv = null;
        this.deathCount = 0;
    }
}

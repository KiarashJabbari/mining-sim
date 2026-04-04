using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistence
{
    void LoadData(GameData data);

    void SaveData(GameData data);

    //use these as templates for saving & loading required data in other files, making sure to put IDataPersistence in the class beside Monobehavior
    //just define the methods again
    //in the methods, for load do this.whateverdata = data.whateverdata, for save do the opposite
    //looks like this:
    //public void SaveData(GameData data)
    //{
    //  data.whateverData = this.whateverData;
    //}
    //any data that is save/loaded must also appear in the GameData.cs file
}

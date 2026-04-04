using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
public class FileDataHandler
{
    //wherever data is saved on the computer
    private string dataDirPath = "";
    //whatever file we're saving to
    private string dataFileName = "";

    public FileDataHandler(string dataDirPath,string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load(string profileID)
    {
        //base case (exit the function if parameters are not met) - if the profileID is null
        if (profileID == null)
        {
            return null;
        }

        // if we compare the file to a person, this fullpath essentially gives an address to look in, and then the name of a specific person living there. 
        string fullPath = Path.Combine(dataDirPath, profileID, dataFileName);
        GameData loadedData = null; // creates a new GameData instance to fill with the loaded data, empty for now
        if (File.Exists(fullPath)) // looks for the person using their name and address, if we continue the analogy
        {
            try
            {
                //load serializsed data from file
                string dataToLoad = "";
                using(FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using(StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                //deserialize gamedaa from json --> c#
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e) 
            {
                Debug.LogError("Error when loading data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(GameData data, string profileID)
    {
        if (profileID == null)
        {
            return;
        }


        string fullPath = Path.Combine(dataDirPath, profileID, dataFileName);
        try
        {
            //create the directory (folder?) that the files gonna be written to if it doesnt exist already (what happens if it already exists?? r we getting 2?)
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            //serialize gameData from c# --> json
            string dataToStore = JsonUtility.ToJson(data, true);

            //write serialized data to file, using 'using' to make sure file connection is severed once reading/writing is done
            //so im assuming filestream is literally connecting the code to the json file which would allow us to then write to it?
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                //initializes writer? im guessing??
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error when saving data to file: " + fullPath + "\n" + e);
        }
    }
    
    //dictionary will connect the string (name of the save) to a GameData object
    public Dictionary<string, GameData> LoadAllProfiles()
    {
        Dictionary<string, GameData> profileDictionary = new Dictionary<string, GameData>();
        //loop over directory names in the path
        IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            string profileID = dirInfo.Name;

            //check if data file exists (defensive programming?)
            // otherwise the folder is not a profile and must be skipped
            string fullPath = Path.Combine(dataDirPath, profileID, dataFileName);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning("skipping [" + profileID + "] directory when loading as it does not contain data.");
                continue;
            }

            //after the check, load game data and put it in the dictionary
            GameData profileData = Load(profileID);

            //more defensive programming making sure the profile data isnt null, and catching it
            if (profileData != null)
            {
                profileDictionary.Add(profileID, profileData);
            }
            else
            {
                Debug.LogError("Attempted to load profile that contained null:" + profileID);
            }
        }
        

        return profileDictionary;
    }

    public string GetMostRecentlyUpdatedProfileID()
    {
        string mostRecentProfileID = null;
        Dictionary<string, GameData> profilesGameData = LoadAllProfiles();
        foreach (KeyValuePair<string, GameData> pair in profilesGameData)
        {
            string profileID = pair.Key;
            GameData gameData = pair.Value;

            if(gameData == null)
            {
                continue;
            }

            //if this is the very first data, set as most recent
            if (mostRecentProfileID == null)
            {
                mostRecentProfileID = profileID;
            }
            //other than that, compare them to see which is more recent
            else
            {
                DateTime mostRecentDateTime = DateTime.FromBinary(profilesGameData[mostRecentProfileID].lastUpdated);
                DateTime newDateTime = DateTime.FromBinary(gameData.lastUpdated);
                // compare and set new value if it is the most recent
                if (newDateTime > mostRecentDateTime)
                {
                    mostRecentProfileID = profileID;
                }
            }
        }
        return mostRecentProfileID;
    }
}

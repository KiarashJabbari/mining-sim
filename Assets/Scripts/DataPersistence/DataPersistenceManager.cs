using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool initializeDataIfNull = false;

    [Header("File Storage Config")] //tf do headers do
    [SerializeField] private string fileName;
    private GameData gameData;

    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;

    private string selectedProfileID = "";

    public static DataPersistenceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("more that one DPM exists, deleting newest");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);

        this.selectedProfileID = dataHandler.GetMostRecentlyUpdatedProfileID();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded called");
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void OnSceneUnloaded(Scene scene)
    {
        Debug.Log("OnSceneUnloaded called");
        SaveGame();
    }


    public void ChangeSelectedProfileID(string newProfileID)
    {
        //update profile for saving and loading
        this.selectedProfileID = newProfileID;
        //load the game
        LoadGame();
    }

    public void NewGame()
    {
        //creates a new instance of the games savable starting values
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        //load saved data from json file using handler
        this.gameData = dataHandler.Load(selectedProfileID);

        //start a new game if data is null & we're allowed to initialize data for debugging
        if(this.gameData == null && initializeDataIfNull)
        {
            NewGame();
        }
        // if no data is in the file, log it and return
        if (this.gameData == null)
        {
            Debug.Log("no save data found in the file, a new game must be started");
            return;
        }
        // push loaded data to other cs files that use it
        foreach (IDataPersistence dataPersistence in dataPersistenceObjects)
        {
            dataPersistence.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        if (this.gameData == null)
        {
            Debug.LogWarning("No data found, a new game must be started before data can be saved");
        }
        // pass data to other scripts
        foreach (IDataPersistence dataPersistence in dataPersistenceObjects)
        {
            dataPersistence.SaveData(gameData);
        }
        //timestampt data to find latest save later
        gameData.lastUpdated = System.DateTime.Now.ToBinary();


        // save data to json file using handler
        dataHandler.Save(gameData, selectedProfileID);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public bool HasGameData()
    {
        return this.gameData != null;
    }
    
    public Dictionary<string, GameData> GetAllProfilesGameData()
    {
        return dataHandler.LoadAllProfiles();
    }
}

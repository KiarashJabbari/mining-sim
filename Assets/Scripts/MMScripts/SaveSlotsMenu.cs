using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
public class SaveSlotsMenu : MonoBehaviour 
{
    [Header("Back Buttons")]
    [SerializeField] private Button backButton;

    private SaveSlot[] saveSlots;

    private bool isLoadingGame = false;

    private void Awake()
    {
        saveSlots = this.GetComponentsInChildren<SaveSlot>();
    }

    private void Start()
    {
        ActivateMenu(true);
    }

    public void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        DisableMenuButtons();
        //update selected profile id to be used for data persistence
        DataPersistenceManager.Instance.ChangeSelectedProfileID(saveSlot.GetProfileID());

        // create a new game?? to initialize data to a clean slate if not loading a game
        if (!isLoadingGame)
        {

            DataPersistenceManager.Instance.NewGame();
        }
        //load the scene, also saving the game
        SceneManager.LoadSceneAsync("SampleScene");
    }

    public void ActivateMenu(bool isLoadingGame)
    {
        this.isLoadingGame = isLoadingGame;
        //load all existing profiles
        Dictionary<string, GameData> profilesGameData = DataPersistenceManager.Instance.GetAllProfilesGameData();

        //loop through every save slot in the ui and set content
        foreach (SaveSlot saveslot in saveSlots)
        {
            GameData profieData = null;
            profilesGameData.TryGetValue(saveslot.GetProfileID(), out profieData);
            saveslot.SetData(profieData);
            if (profieData == null && isLoadingGame)
            {
                saveslot.SetInteractable(false);
            }
            else
            {
                saveslot.SetInteractable(true);
            }
        }

    }

    public void BackButton()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    private void DisableMenuButtons()
    {
        foreach (SaveSlot saveslot in saveSlots)
        {
            saveslot.SetInteractable(false);
        }
        backButton.interactable = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MMButtons : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private Button loadGameButton;
    private void Start()
    {
        if (!DataPersistenceManager.Instance.HasGameData())
        {
            continueGameButton.interactable = false;
            loadGameButton.interactable = false;
        }
    }
    public void NewGameButton()
    {
        DisableAllButtons();
        Debug.Log("New Game Started");
        //create new game + initialize gamteData
        DataPersistenceManager.Instance.NewGame();
        //Loads gameplay scene, also saves bc OnSceneUnLoaded is called
        SceneManager.LoadScene("SampleScene"); //i used LoadScene before, whats Async for?
    }

    public void ExitButton()
    {
        Application.Quit();
        DisableAllButtons();
        Debug.Log("Quit Game");
        
    }

    public void ContinueButton()
    {
        DisableAllButtons();
        Debug.Log("Back in Business Boys");
        // loads gameplay scene, then loading game by triggering OnSceneLoaded
        SceneManager.LoadScene("SampleScene");
        
    }

    public void LoadGameButton()
    {
        DisableAllButtons();
        Debug.Log("going to save selector");
        SceneManager.LoadScene("SaveMenu");
    }

    private void DisableAllButtons()
    {
        continueGameButton.interactable = false;
        newGameButton.interactable = false;
        exitGameButton.interactable = false;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    [SerializeField] public GameObject mainMenuPanel;
    [SerializeField] public GameObject settingsPanel;
    [SerializeField] public GameObject infoPanel;

    public void Start()
    {

    }

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void OpenSettings()
    {
        Debug.Log("Opened settings");
        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void OpenInfo()
    {
        Debug.Log("Opened info");
        infoPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void BackToMainMenu()
    {
        infoPanel.SetActive(false);
        settingsPanel.SetActive(false);
        
        mainMenuPanel.SetActive(true);
    }
}
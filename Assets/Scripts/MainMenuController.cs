using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Mode-Specific Panels")]
    public GameObject pvpPanel;
    public GameObject pvaiPanel;

    [Header("Player Names")]
    public TMP_InputField p1NameInput;
    public TMP_InputField p2NameInput;

    [Header("Mode / Map")]
    public TMP_Text modeText;
    public TMP_Text mapText;
    public TMP_Text roundText;
    public GameObject error;
    private int modeIndex = 0;
    private int mapIndex = 0;
    private int roundIndex = 0;
    private bool ready = false;

    private string[] modes = { "PvP", "PvAI" };
    private string[] maps = { "Void", "Bar" };
    private int[] rounds = { 3, 5, 7, 9 };

    [Header("Settings SO")]
    public GameSettings settings;

    void Update()
    {
        UpdateSettings();   
    }

    void UpdateSettings()
    {
        settings.p1Name = p1NameInput.text;
        settings.p2Name = p2NameInput.text;
        settings.totalRounds = rounds[roundIndex];
    }
    public void PlayClicked()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        UpdateModeText();
        UpdateModePanels();
        UpdateMapText();
    }

    public void BackClicked()
    {
        settingsPanel.SetActive(false);
        error.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ModeLeft()
    {
        modeIndex = (modeIndex - 1 + modes.Length) % modes.Length;
        UpdateModeText();
        UpdateModePanels();
    }

    public void ModeRight()
    {
        modeIndex = (modeIndex + 1) % modes.Length;
        UpdateModeText();
        UpdateModePanels();
    }

    public void MapLeft() { mapIndex = (mapIndex - 1 + maps.Length) % maps.Length; UpdateMapText(); }
    public void MapRight() { mapIndex = (mapIndex + 1) % maps.Length; UpdateMapText(); }

    public void RoundLeft() { roundIndex = (roundIndex - 1 + rounds.Length) % rounds.Length; UpdateRoundText(); }
    public void RoundRight() { roundIndex = (roundIndex + 1) % rounds.Length; UpdateRoundText(); }

    void UpdateModeText() { modeText.text = modes[modeIndex]; }
    void UpdateMapText() { mapText.text = maps[mapIndex]; }

    void UpdateRoundText() { roundText.text = rounds[roundIndex].ToString(); }

    void UpdateModePanels()
    {
        if (modes[modeIndex] == "PvP")
        {
            pvpPanel.SetActive(true);
            pvaiPanel.SetActive(false);
        }
        else // PvAI
        {
            pvpPanel.SetActive(false);
            pvaiPanel.SetActive(true);
        }
    }
    void nameCheck()
    {
        if(p1NameInput.text == "" || p2NameInput.text == "" )
        {
            Debug.Log("Please give a name to each player.");
            error.SetActive(true);
            ready = false;
        }
        else
        {
            Debug.Log("Good to go!");
            ready = true;
        }
    }

    public void StartPVP()
    {
        nameCheck();
        if (ready) 
        {
            SceneManager.LoadScene("pvpScene");
            Debug.Log("Scene loaded, GameController Start() executed!");
        }
    }
    public void QuitGame()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

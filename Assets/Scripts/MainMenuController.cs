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
    private int modeIndex = 0;
    private int mapIndex = 0;

    private string[] modes = { "PvP", "PvAI" };
    private string[] maps = { "Void", "Bar" };

    [Header("Settings SO")]
    public GameSettings settings;

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

    void UpdateModeText() { modeText.text = modes[modeIndex]; }
    void UpdateMapText() { mapText.text = maps[mapIndex]; }

    // 🔥 NEW: Toggles the correct panel for the current mode
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

    public void StartPVP()
    {
        SceneManager.LoadScene("pvpScene");
        Debug.Log("Scene loaded, GameController Start() executed!");
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

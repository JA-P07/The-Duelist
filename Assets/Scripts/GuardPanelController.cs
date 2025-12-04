using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GuardPanelController : MonoBehaviour
{
    public PlayerController P1;
    public PlayerController P2;
    public GameController gameController;
    public TMP_Text P1Name;
    public TMP_Text P2Name;
    public GameSettings settings;
    // Start is called before the first frame update
    void Update()
    {
        P1Name.text = settings.p1Name;
        P2Name.text = settings.p2Name;
    }
    void OffensiveChosen (PlayerController player) 
    {
        player.StartingGuard = 0;
        player.ready = true;
    }

    void DefensiveChosen(PlayerController player)
    {
        player.StartingGuard = 1;
        player.ready = true;
    }

    public void P1Off()
    {
        Debug.Log("Player 1 chose Offensive guard");
        OffensiveChosen(P1);
    }

    public void P1Def()
    {
        Debug.Log("Player 1 chose Defensive guard");
        DefensiveChosen(P1);
    }

    public void P2Off()
    {
        Debug.Log("Player 2 chose Offensive guard");
        OffensiveChosen(P2);
    }

    public void P2Def()
    {
        Debug.Log("Player 2 chose Defensive guard");
        DefensiveChosen(P2);
    }
}

using System;
using System.Collections;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum ActionType { None, Thrust, SlashLeft, SlashRight, Feint, Brace}
public enum GameState { Start, WaitingForActions, ResolvingActions, ReactMode, GameOver }

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerController player1;
    [SerializeField] private PlayerController player2;
    [SerializeField] private SimpleReactController reactController;
    public GameSettings settings;
    public int currentRound;
    public int totalRounds;
    private int player1Score = 0;
    private int player2Score = 0;

    public TextMeshPro roundText;
    public TextMeshPro P1Name;
    public TextMeshPro P2Name;
    public GameObject infoPanel;
    public GameObject PausePanel;
    public GameObject EndPanel;
    public GameObject GuardPanel;
    public GameObject PlayersObject;
    public GameObject UI;


    private ActionType player1Action = ActionType.None;
    private ActionType player2Action = ActionType.None;
    private GameState currentState = GameState.WaitingForActions;

    private bool player1ActionChosen = false;
    private bool player2ActionChosen = false;

    private bool info = false;
    private bool pause = false;

    void Start()
    {
        startGame();
    }

    public void startGame()
    {
        EndPanel.SetActive(false);
        // Reset Time Scale in case game was paused or info was open
        Time.timeScale = 1f;
        pause = false;
        info = false;
        PausePanel.SetActive(false);
        infoPanel.SetActive(false);

        // Reset internal game state
        currentState = GameState.WaitingForActions;

        // Clear QTE subscriptions
        if (reactController != null)
            reactController.onComplete = null;

        // Cancel all pending invokes (like StartNextRound)
        CancelInvoke();

        // Reset match data
        settings.WinningPlayerName = "";
        player1.StartingGuard = 2;
        player2.StartingGuard = 2;
        player1Score = 0;
        player2Score = 0;

        // Reset players
        currentRound = 1;
        player1.HP = settings.startingHP;
        player2.HP = settings.startingHP;
        player1.ResetForNewTurn();
        player2.ResetForNewTurn();

        // Reset UI
        P1Name.text = settings.p1Name;
        P2Name.text = settings.p2Name;
        totalRounds = settings.totalRounds;
        roundText.text = currentRound + " of " + totalRounds;

        // Reset action flags
        player1Action = ActionType.None;
        player2Action = ActionType.None;
        player1ActionChosen = false;
        player2ActionChosen = false;

        // Ready to go
        ChooseGuard();
    }

    private void Update()
    {
        if (player2.ready == true && player1.ready == true)
        {
            GuardPanel.SetActive(false);
            player1.ready = false;
            player2.ready = false;
            UI.SetActive(true);
            StartTurn();
        }
        if (Input.GetKeyDown("i"))
        {
            Info();
        }
        if (Input.GetKeyDown("escape"))
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (pause)
        {
            PausePanel.SetActive(false);
            pause = false;
            Time.timeScale = 1;
        }
        else if (!info)
        {
            Time.timeScale = 0;
            pause = true;
            PausePanel.SetActive(true);
        }
    }

    public void Info()
    {
        if (info)
        {
            infoPanel.SetActive(false);
            info = false;
            Time.timeScale = 1;
            UI.SetActive(true);
        }
        else if (!pause)
        {
            Time.timeScale = 0;
            info = true;
            infoPanel.SetActive(true);
            UI.SetActive(false);
        }
    }

    void ChooseGuard()
    {
        UI.SetActive(false);
        GuardPanel.SetActive(true);
    }

    void StartTurn()
    {
        player1.ResetForNewTurn();
        player2.ResetForNewTurn();

        currentState = GameState.WaitingForActions;
        Debug.Log("New turn started. Waiting for actions...");

        player1Action = ActionType.None;
        player2Action = ActionType.None;

        player1ActionChosen = false;
        player2ActionChosen = false;

        // Tell players to choose their actions
    }

    // Called by player scripts when they pick an action
    public void SubmitAction(PlayerController player, ActionType action)
    {
        if (currentState != GameState.WaitingForActions) return;
        if (player == player1)
        {
            player1Action = action;
            player1ActionChosen = true;
            Debug.Log("Player1 chose: " + action);
        }
        else if (player == player2)
        {
            player2Action = action;
            player2ActionChosen = true;
            Debug.Log("Player2 chose: " + action);
        }

        if (player1ActionChosen && player2ActionChosen)
        {
            currentState = GameState.ResolvingActions;
            ReactModeCheck();
        }
    }

    void ReactModeCheck()
    {
        // Determine who is attacking faster
        if (player1.isAttack && !player2.isAttack && reactController != null ||
            player1.isAttack && player2.isAttack && player1.attackSpeed > player2.attackSpeed && reactController != null)
        {
            EnterReactMode(player2, player1);
        }
        else if (player2.isAttack && !player1.isAttack && reactController != null ||
                 player2.isAttack && player1.isAttack && player2.attackSpeed > player1.attackSpeed && reactController != null)
        {
            EnterReactMode(player1, player2);
        }
        else if (reactController != null && player1.isAttack && player2.isAttack)
        {
            randomizePlayer();
        }
        else if (reactController != null && !player1.isAttack && !player2.isAttack)
        {
            ResolveActions();
        }
    }

    void randomizePlayer()
    {
        System.Random rd = new System.Random();
        int playerNum = rd.Next(1, 3);

        if (playerNum == 1)
        {
            EnterReactMode(player1, player2);
        }
        else if (playerNum == 2)
        {
            EnterReactMode(player2, player1);
        }
    }

    void EnterReactMode(PlayerController reactingPlayer, PlayerController attackingPlayer)
    {
        currentState = GameState.ReactMode;

        Debug.Log($"{reactingPlayer.name} enters React Mode against {attackingPlayer.name}!");

        // Clear any leftover subscriptions
        reactController.onComplete = null;

        // Subscribe once
        reactController.onComplete = (success, perfect) =>
        {
            HandleReactComplete(success, perfect, reactingPlayer, attackingPlayer);
        };

        if (attackingPlayer.name == "player1")
        {
            // Spawn QTE above player2
            reactController.StartQTE(1f, 2, reactingPlayer.transform);
        }
        else if (attackingPlayer.name == "player2")
        {
            // Spawn QTE above player1
            reactController.StartQTE(1f, 1, reactingPlayer.transform);
        }
    }

    void HandleReactComplete(bool success, bool perfect, PlayerController reactingPlayer, PlayerController attackingPlayer)
    {
        if (success)
        {
            Debug.Log(perfect ? "Perfect reaction!" : "Good reaction!");

            PostReaction(success, perfect, reactingPlayer, attackingPlayer);
        }
        else
        {
            Debug.Log($"{reactingPlayer.name} failed to react — full damage!");
            reactingPlayer.reacting = false;
            ResolveActions();
        }
    }

    void PostReaction(bool success, bool perfect, PlayerController reactingPlayer, PlayerController attackingPlayer)
    {
        if (perfect)
        {
            Debug.Log(reactingPlayer + " makes a counterattack");
            attackingPlayer.damage = 0;
            reactingPlayer.damage = 15;
            ResolveActions();
        }

        else if (success && !perfect)
        {
            Debug.Log(reactingPlayer + " blocks, damage prevented");
            attackingPlayer.damage = 0;
            reactingPlayer.damage = 0;
            ResolveActions();
        }
    }

    void ResolveActions()
    {
        Debug.Log("Resolving actions...");
        Debug.Log("Player1 action: " + player1Action);
        Debug.Log("Player2 action: " + player2Action);

        // Calculate damage dealt from each player to the other
        float damageFromP1 = player1.damage * player1.DamageDealt;
        float damageFromP2 = player2.damage * player2.DamageDealt;

        // Apply damage considering defender's DamageTaken multiplier
        float effectiveDamageToP2 = damageFromP1 * player2.DamageTaken;
        float effectiveDamageToP1 = damageFromP2 * player1.DamageTaken;

        player2.HP -= effectiveDamageToP2;
        player1.HP -= effectiveDamageToP1;

        Debug.Log($"Player1 deals {effectiveDamageToP2} damage to Player2. Player2 HP: {player2.HP}");
        Debug.Log($"Player2 deals {effectiveDamageToP1} damage to Player1. Player1 HP: {player1.HP}");

        bool p1Hit = effectiveDamageToP2 > 0;
        bool p2Hit = effectiveDamageToP1 > 0;

        if (p1Hit || p2Hit)
        {
            EndRound(p1Hit, p2Hit);
            return;
        }
        else
        {
            StartTurn();
        }
    }

    void EndRound(bool p1Hit, bool p2Hit)
    {

        if (p1Hit && !p2Hit)
        {
            player1Score++;
            player2.takeHit(); // only runs if P1 hit and P2 didn’t
        }

        if (p2Hit && !p1Hit)
        {
            player2Score++;
            player1.takeHit(); // only runs if P2 hit and P1 didn’t
        }
        if (p1Hit && p2Hit)
        {
            // Double hit — up to you how to score it
        }

        Debug.Log($"Round {currentRound} over!");
        Debug.Log($"Score: P1 {player1Score} - {player2Score} P2");

        if (currentRound >= totalRounds)
        {
            EndMatch(false, 0);
        }
        else if (player1.HP <= 0)
        {
            Debug.Log("Player 1 has been defeated!");
            EndMatch(true, 1);
        }
        else if (player2.HP <= 0)
        {
            Debug.Log("Player 2 has been defeated!");
            EndMatch(true, 2);
        }
        else
        {
            Invoke(nameof(StartNextRound), 2f);
        }
    }
    void StartNextRound()
    {
        currentRound++;
        roundText.text = currentRound + " of " + totalRounds;
        StartTurn();
    }

    void EndMatch(bool playerDied, int Dead)
    {
        if (playerDied)
        {
            if (playerDied && Dead == 1) //P1 dies
            {
                settings.WinningPlayerName = settings.p1Name;
            }
            else if (playerDied && Dead == 2) //P2 dies
            {
                settings.WinningPlayerName = settings.p2Name;
            }
        }
        else if (!playerDied || Dead == 0) 
        {
            if (player1Score > player2Score) //P1 wins via points
            {
                settings.WinningPlayerName = settings.p1Name;
            }
            else if (player2Score > player1Score) //P2 wins via points
            {
                settings.WinningPlayerName = settings.p2Name;
            }
        }
        EndPanel.SetActive(true);
        UI.SetActive(false);
    }

}

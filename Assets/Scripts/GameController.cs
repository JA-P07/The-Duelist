using System;
using System.Collections;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum ActionType { None, Thrust, SlashLeft, SlashRight, Feint, Brace}
public enum GameState { WaitingForActions, ResolvingActions, ReactMode, GameOver }

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

    private ActionType player1Action = ActionType.None;
    private ActionType player2Action = ActionType.None;
    private GameState currentState = GameState.WaitingForActions;

    private bool player1ActionChosen = false;
    private bool player2ActionChosen = false;

    private bool info;
    private bool pause;

    void Start()
    {
        currentRound = 1;
        player1.HP = settings.startingHP;
        player2.HP = settings.startingHP;
        P1Name.text = settings.p1Name;
        P2Name.text = settings.p2Name;
        totalRounds = settings.totalRounds;
        roundText.text = currentRound + " of " + totalRounds;
        StartTurn();
    }

    private void Update()
    {
        if (Input.GetKeyDown("i"))
        {
            if (info)
            {
                infoPanel.SetActive(false);
                info = false;
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;
                info = true;
                infoPanel.SetActive(true);
            }
        }
        if (Input.GetKeyDown("escape"))
        {
            if (pause)
            {
                PausePanel.SetActive(false);
                pause = false;
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;
                pause = true;
                PausePanel.SetActive(true);
            }
        }
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
        if (player1.isAttack && (!player2.isAttack || player1.attackSpeed > player2.attackSpeed))
        {
            EnterReactMode(player2, player1);
        }
        else if (player2.isAttack && (!player1.isAttack || player2.attackSpeed > player1.attackSpeed))
        {
            EnterReactMode(player1, player2);
        }
        else
        {
            ResolveActions();
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
            reactingPlayer.damage = 30;
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

        if (p1Hit && !p2Hit) player1Score++;
        if (p2Hit && !p1Hit) player2Score++;
        if (p1Hit && p2Hit)
        {
            // Double hit — up to you how to score it
        }

        Debug.Log($"Round {currentRound} over!");
        Debug.Log($"Score: P1 {player1Score} - {player2Score} P2");

        if (currentRound >= totalRounds)
        {
            EndMatch();
        }
        else if (player1.HP <= 0)
        {
            Debug.Log("Player 1 has been defeated!");
            EndMatch();
        }
        else if (player2.HP <= 0)
        {
            Debug.Log("Player 2 has been defeated!");
            EndMatch();
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

    void EndMatch()
    {
        Debug.Log("Game ended! More logic will be added soon!");
    }

}

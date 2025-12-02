using System.Collections.Generic;
using System.Collections;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameController gameController;

    [SerializeField] public KeyCode[] relevantKeys;

    private List<KeyCode> inputBuffer = new List<KeyCode>();
    private float comboMaxTime = 0.5f;
    private float lastInputTime = 0f;
    public float HP = 200;
    public float attackSpeed;
    public bool isAttack;
    public float momentum;
    public float damage;
    public int TP;
    private bool defensive;
    private bool offensive;
    public float DamageDealt;
    public float DamageTaken;
    public int StartingGuard;
    [HideInInspector] public bool reacting = false;
    [HideInInspector] public bool counterMode = false;
    [HideInInspector] public bool counterActionChosen = false;
    public string direction;
    public int bonus;
    public int cost;
    private SpriteRenderer Sprite;


    // Combos defined by indexes into relevantKeys
    private int[] thrustCombo = new int[] { 0, 0 };       // press relevantKeys[0] twice
    private int[] leftCombo = new int[] { 3, 3 };
    private int[] rightCombo = new int[] { 2, 2 };
    private int[] braceCombo = new int[] { 1, 1 };    // press relevantKeys[1] twice

    private void Start()
    {
        Sprite = GetComponent<SpriteRenderer>();
        TP = 10;
        momentum = 1.0f;
        direction = "any";
        if (StartingGuard == 1)
        {
            defensive = true;
            offensive = false;
        }
        else
        {
            offensive = true;
            defensive = false;
        }
    }
    public void checkGuard()
    {
        if (offensive == true)
        {
            defensive = false;
            DamageDealt = 1.0f;
            DamageTaken = 1.5f;
            bonus = 1;
        }
        else if (defensive == true)
        {
            offensive = false;
            DamageDealt = 0.5f;
            DamageTaken = 0.5f;
            bonus = 2;
        }
    }
    public void ResetForNewTurn()
    {
        reacting = false;
        isAttack = false;
        direction = "any";
        damage = 0;
    TP = TP + bonus;
    }

    public void StartCounterMode()
    {
        counterMode = true;
        counterActionChosen = false;
        reacting = false; // make sure react mode is off
    }

    public void EndCounterMode()
    {
        counterMode = false;
        counterActionChosen = false;
    }

    void TPTransaction(int cost)
    {
        TP = TP - cost;
    }

    void TPCheck (int cost)
    {
        if (TP < cost)
        {
            Debug.Log("Can't perform action");
        }
        else
        {
            TPTransaction(cost);
        }
    }

    void Update()
    {
        if (TP > 20)
        {
            TP = 20;
        }
        if (TP < 0)
        {
            TP = 0;
        }
        checkGuard();
        if (Input.anyKeyDown)
        {
            foreach (KeyCode key in relevantKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    inputBuffer.Add(key);
                    lastInputTime = Time.time;
                    break;
                }
            }
        }

        if (Time.time - lastInputTime > comboMaxTime)
        {
            inputBuffer.Clear();
        }
            // normal combat actions
            if (CheckCombo(thrustCombo)) { PerformThrust(); inputBuffer.Clear(); }
            if (CheckCombo(leftCombo)) { CheckLeft(); inputBuffer.Clear(); }
            if (CheckCombo(rightCombo)) { CheckRight(); inputBuffer.Clear(); }
            if (CheckCombo(braceCombo)) { PerformBrace(); inputBuffer.Clear(); }
    }

    bool CheckCombo(int[] combo)
    {
        if (inputBuffer.Count < combo.Length) return false;

        for (int i = 0; i < combo.Length; i++)
        {
            if (inputBuffer[inputBuffer.Count - combo.Length + i] != relevantKeys[combo[i]])
                return false;
        }
        return true;
    }

    void CheckLeft()
    {
        if (offensive == true)
        {
            TPCheck(8);
            Debug.Log(name + " performs a slash to the left");
            damage = 40;
            attackSpeed = 1.0f * momentum;
            isAttack = true;
            direction = "left";
            gameController.SubmitAction(this, ActionType.SlashLeft);
            defensive = true;
            offensive = false;
        }
        else if (offensive == false)
        {
            TPCheck(0);
            offensive = true;
            defensive = false;
            isAttack = false;
            gameController.SubmitAction(this, ActionType.None);
        }
    }
    void CheckRight()
    {
        if (defensive == true)
        {
            TPCheck(8);
            Debug.Log(name + " performs a slash to the right");
            damage = 40;
            attackSpeed = 1.0f * momentum;
            isAttack = true;
            direction = "right";
            gameController.SubmitAction(this, ActionType.SlashRight);
            defensive = false;
            offensive = true;
        }
        else if (defensive == false)
        {
            TPCheck(0);
            defensive = true;
            offensive = false;
            isAttack=false;
            gameController.SubmitAction(this, ActionType.None);
        }
    }
    void PerformThrust()
    {
        TPCheck(5);
        Debug.Log(name + " performs Thrust");
        gameController.SubmitAction(this, ActionType.Thrust);
        direction = "any";
        damage = 15;
        attackSpeed = 1.1f * momentum;
        isAttack = true;
    }

    void PerformBrace()
    {
        TPCheck(0);
        Debug.Log(name + " gets ready for the incoming attack");
        TP = TP + 2;
        attackSpeed = 0.5f * momentum;
        isAttack = false;
        if (offensive == true)
        {
            offensive = false;
            defensive = true;
        }
        gameController.SubmitAction(this, ActionType.Brace);
    }
}


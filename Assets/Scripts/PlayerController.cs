using System.Collections.Generic;
using System.Collections;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public GameController gameController;

    [SerializeField] public KeyCode[] relevantKeys;

    private List<KeyCode> inputBuffer = new List<KeyCode>();
    private float comboMaxTime = 0.5f;
    private float lastInputTime = 0f;
    public float HP;
    public float attackSpeed;
    public bool isAttack;
    public float momentum; // not ready yet I think
    public float damage;
    public int TP;
    private bool defensive;
    private bool offensive;
    public float DamageDealt;
    public float DamageTaken;
    public int StartingGuard; //choosing guards
    [HideInInspector] public bool reacting = false;
    [HideInInspector] public bool counterMode = false;
    [HideInInspector] public bool counterActionChosen = false;
    private bool chosen;
    public bool ready; //choosing guards
    public int bonus; //TP
    public int cost; //TP
    public TextMeshPro ActionText;
    private SpriteRenderer Sprite;
    private Animator animator;



    // Combos defined by indexes into relevantKeys
    private int[] thrustCombo = new int[] { 0, 0 };       // press relevantKeys[0] twice
    private int[] leftCombo = new int[] { 3, 3 };
    private int[] rightCombo = new int[] { 2, 2 };
    private int[] braceCombo = new int[] { 1, 1 };    // press relevantKeys[1] twice

    private void Start()
    {
        //ActionText.text = "Action: jeejee";
        animator = GetComponent<Animator>();
        Sprite = GetComponent<SpriteRenderer>();
        TP = 10;
        momentum = 1.0f;
        if (StartingGuard == 1)
        {
            defensive = true;
            offensive = false;
        }
        else if (StartingGuard == 0)
        {
            offensive = true;
            defensive = false;
        }
    }

    public void setGuard()
    {
        if (StartingGuard == 1)
        {
            offensive = false;
            defensive = true;
            StartingGuard = 2;
            TP = 10;
        }
        else if (StartingGuard == 0)
        {
            offensive = true;
            defensive = false;
            StartingGuard = 2;
            TP = 10;
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
            animator.SetInteger("GuardState", 0);
        }
        else if (defensive == true)
        {
            offensive = false;
            DamageDealt = 0.5f;
            DamageTaken = 0.5f;
            bonus = 2;
            animator.SetInteger("GuardState", 1);
        }
    }
    public void takeHit()
    {
        Debug.Log(name + " taking hit, guard: " + (offensive ? "Offensive" : "Defensive"));
        if (offensive)
            animator.SetInteger("DmgTaken", 1);
        else
            animator.SetInteger("DmgTaken", 2);
    }


    public void ResetForNewTurn()
    {
        animator = GetComponent<Animator>();
        Debug.Log(animator);
        animator.SetInteger("DmgTaken", 0);
        reacting = false;
        counterMode = false;
        counterActionChosen = false; 
        isAttack = false;
        chosen = false;
        damage = 0;
        inputBuffer.Clear();
        lastInputTime = 0;
        TP = TP + bonus;
        ActionText.text = "Action: None";
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
            chosen = false;
            Debug.Log("Can't perform action");
            ActionText.text = "Not enough Tempo, do something else";
        }
        else
        {
            TPTransaction(cost);
            chosen = true;
        }
    }

    void Update()
    {
        HP = Mathf.Round(HP);
        if (TP > 20)
        {
            TP = 20;
        }
        if (TP < 0)
        {
            TP = 0;
        }
        setGuard();
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

        if (!chosen && Time.time - lastInputTime > comboMaxTime)
        {
            inputBuffer.Clear();
        }

        if (!chosen) { 
            // normal combat actions
            if (CheckCombo(thrustCombo)) { PerformThrust(); inputBuffer.Clear(); }
            if (CheckCombo(leftCombo)) { CheckLeft(); inputBuffer.Clear(); }
            if (CheckCombo(rightCombo)) { CheckRight(); inputBuffer.Clear(); }
            if (CheckCombo(braceCombo)) { PerformBrace(); inputBuffer.Clear(); }

            else
            {
                //ActionText.text = "Invalid input, try again.";
            }
        }
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
        if (offensive && !defensive)
        {
            TPCheck(8);
            if (!chosen)
                return;
            Debug.Log(name + " performs a slash to the left");
            damage = 20;
            attackSpeed = 1.0f * momentum;
            isAttack = true;
            defensive = true;
            offensive = false;
            ActionText.text = "Action: Slashing (left)";
            gameController.SubmitAction(this, ActionType.SlashLeft);
        }
        else if (!offensive && defensive)
        {
            TPCheck(0);
            if (!chosen)
                return;
            offensive = true;
            defensive = false;
            isAttack = false; 
            ActionText.text = "Action: Changing guard";
            gameController.SubmitAction(this, ActionType.None);
        }
    }
    void CheckRight()
    {
        if (defensive && !offensive)
        {
            TPCheck(8);
            if (!chosen)
                return;
            Debug.Log(name + " performs a slash to the right");
            damage = 20;
            attackSpeed = 1.0f * momentum;
            isAttack = true;
            defensive = false;
            offensive = true; 
            ActionText.text = "Action: Slashing (right)";
            gameController.SubmitAction(this, ActionType.SlashRight);
        }
        else if (!defensive && offensive)
        {
            TPCheck(0);
            if (!chosen)
                return;
            defensive = true;
            offensive = false;
            isAttack=false;
                ActionText.text = "Action: Changing guard";
                gameController.SubmitAction(this, ActionType.None);
        }
    }
    void PerformThrust()
    {
        TPCheck(5);
        if (!chosen)
            return;
        Debug.Log(name + " performs Thrust");
        damage = 8;
        attackSpeed = 1.1f * momentum;
        isAttack = true;
            ActionText.text = "Action: Thrusting";
            gameController.SubmitAction(this, ActionType.Thrust);
    }

    void PerformBrace()
    {
        TPCheck(0);
        if (!chosen)
            return;
        Debug.Log(name + " gets ready for the incoming attack");
        TP = TP + 2;
        attackSpeed = 0.5f * momentum;
        isAttack = false;
        if (offensive == true)
        {
            offensive = false;
            defensive = true;
        }
            ActionText.text = "Action: Bracing";
            gameController.SubmitAction(this, ActionType.Brace);
    }
}


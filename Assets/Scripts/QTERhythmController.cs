using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class QTERhythmController : MonoBehaviour
{
    public event Action<bool, bool, PlayerController> OnReactComplete;

    private PlayerController reactingPlayer;

    [Header("References")]
    public GameObject notePrefab;
    public Transform spawnPoint;
    public Transform hitZone;

    [Header("Note Timing")]
    public float travelTime = 1.2f;
    public float timeBetweenNotes = 0.35f;
    public int notesPerQTE = 6;

    [Header("Hit Windows")]
    public float perfectWindow = 0.09f;
    public float goodWindow = 0.17f;

    [Header("Scoring")]
    public int scorePerfect = 100;
    public int scoreGood = 50;
    public int perfectThreshold = 450;
    public int successThreshold = 250;

    private bool qteActive = false;

    private List<Note> activeNotes = new List<Note>();

    private void Update()
    {
        if (!qteActive) return;

        CheckPlayerInput();
        CleanupExpiredNotes();
    }

    // Called by GameController
    public void StartReactMode(PlayerController player, float ignoredDuration)
    {
        reactingPlayer = player;
        qteActive = true;

        StartCoroutine(RunQTE());
    }

    private IEnumerator RunQTE()
    {
        activeNotes.Clear();
        float start = Time.time;

        // Create sequence
        List<(int dir, float spawn)> seq = new List<(int dir, float spawn)>();
        for (int i = 0; i < notesPerQTE; i++)
        {
            int dir = UnityEngine.Random.Range(0, 4); // 0=Left,1=Right,2=Up,3=Down
            float spawnAt = start + i * timeBetweenNotes;
            seq.Add((dir, spawnAt));
        }

        // Spawn notes
        foreach (var n in seq)
        {
            float spawnMoment = n.spawn - travelTime;
            float wait = spawnMoment - Time.time;

            if (wait > 0) yield return new WaitForSeconds(wait);

            SpawnNote(n.dir);
        }

        // Wait until all notes handled
        while (activeNotes.Count > 0)
            yield return null;

        // Scoring
        QTEScore final = CalculateScore();

        bool success = final.score >= successThreshold;
        bool perfect = final.score >= perfectThreshold;

        qteActive = false;
        OnReactComplete?.Invoke(success, perfect, reactingPlayer);
    }

    private void SpawnNote(int direction)
    {
        GameObject go = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity, this.transform);
        Note note = go.GetComponent<Note>();
        note.Initialize(direction, hitZone.position, travelTime);

        activeNotes.Add(note);
    }

    private void CheckPlayerInput()
    {
        // Map 4 inputs to direction indexes
        KeyCode[] keys = reactingPlayer.relevantKeys;

        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                // Try to match earliest active note with same direction
                Note best = null;
                float bestTime = Mathf.Infinity;

                foreach (Note n in activeNotes)
                {
                    if (n.directionIndex != i) continue;

                    float dt = Mathf.Abs(Time.time - n.scheduledHitTime);
                    if (dt < bestTime)
                    {
                        best = n;
                        bestTime = dt;
                    }
                }

                if (best != null)
                {
                    JudgeNoteHit(best, bestTime);
                    break;
                }
            }
        }
    }

    private void JudgeNoteHit(Note note, float dt)
    {
        if (dt <= perfectWindow)
            note.result = NoteResult.Perfect;
        else if (dt <= goodWindow)
            note.result = NoteResult.Good;
        else
            note.result = NoteResult.Miss;

        activeNotes.Remove(note);
        Destroy(note.gameObject);
    }

    private void CleanupExpiredNotes()
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note n = activeNotes[i];

            if (Time.time - n.scheduledHitTime > goodWindow)
            {
                n.result = NoteResult.Miss;
                Destroy(n.gameObject);
                activeNotes.RemoveAt(i);
            }
        }
    }

    private QTEScore CalculateScore()
    {
        int score = 0;
        int perfect = 0;
        int good = 0;
        int miss = 0;

        foreach (Note n in activeNotes)
        {
            if (n.result == NoteResult.Perfect)
            {
                score += scorePerfect;
                perfect++;
            }
            else if (n.result == NoteResult.Good)
            {
                score += scoreGood;
                good++;
            }
            else
            {
                miss++;
            }
        }

        QTEScore res = new QTEScore(score, perfect, good, miss);
        return res;
    }
}

public enum NoteResult { Unjudged, Perfect, Good, Miss }

public class QTEScore
{
    public int score;
    public int perfect;
    public int good;
    public int miss;

    public QTEScore(int s, int p, int g, int m)
    {
        score = s;
        perfect = p;
        good = g;
        miss = m;
    }
}

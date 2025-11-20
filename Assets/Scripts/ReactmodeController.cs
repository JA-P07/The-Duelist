using UnityEngine;
using System;
using System.Collections;

public class ReactmodeController : MonoBehaviour
{
    public event Action<bool, bool, PlayerController> OnReactComplete;

    private PlayerController reactingPlayer;
    private bool isActive = false;

    // Core settings
    [SerializeField] private float timeLimit = 1.0f;
    [SerializeField] private float perfectWindow = 0.15f; // e.g. ±0.15s around midpoint

    private float timer = 0f;
    private float targetTime; // the "perfect" moment within the window

    public void StartReactMode(PlayerController player, float duration)
    {
        reactingPlayer = player;
        timeLimit = duration;

        // start timing
        timer = 0f;
        targetTime = timeLimit / 2f; // you can randomize this later for unpredictability
        isActive = true;

        StartCoroutine(ReactTimer());
        Debug.Log($"{player.name} started React Mode!");
    }

    private IEnumerator ReactTimer()
    {
        bool reacted = false;
        bool perfect = false;

        while (timer < timeLimit)
        {
            timer += Time.deltaTime;

            foreach (KeyCode key in reactingPlayer.relevantKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    float diff = Mathf.Abs(timer - targetTime);

                    if (diff <= perfectWindow)
                    {
                        perfect = true;
                        reacted = true;
                        Debug.Log($"[{reactingPlayer.name}] PERFECT reaction at {timer:F2}s!");
                    }
                    else if (diff <= perfectWindow * 2)
                    {
                        reacted = true;
                        Debug.Log($"[{reactingPlayer.name}] Good reaction at {timer:F2}s!");
                    }
                    else
                    {
                        Debug.Log($"[{reactingPlayer.name}] Too early/late ({timer:F2}s).");
                    }

                    EndReactMode(reacted, perfect);
                    yield break;
                }
            }

            yield return null;
        }

        // time's up = failed
        EndReactMode(false, false);
    }

    void EndReactMode(bool success, bool perfect)
    {
        if (!isActive) return;
        isActive = false;
        Debug.Log($"ReactMode ended. Success={success}, Perfect={perfect}");
        OnReactComplete?.Invoke(success, perfect, reactingPlayer);
    }
}

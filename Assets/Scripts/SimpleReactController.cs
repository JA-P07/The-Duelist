using UnityEngine;
using System;

public class SimpleReactController : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject circlePrefab;      // prefab for shrinking circle
    public GameObject hitMarkerPrefab;   // prefab for hit marker

    [Header("Key Prefabs")]
    public GameObject upArrowPrefab;
    public GameObject downArrowPrefab;
    public GameObject leftArrowPrefab;
    public GameObject rightArrowPrefab;
    public GameObject wPrefab;
    public GameObject aPrefab;
    public GameObject sPrefab;
    public GameObject dPrefab;

    [Header("Timing")]
    public float duration = 1f;
    public float perfectWindow = 0.15f;
    public float goodWindow = 0.3f;

    [Header("Scale")]
    public float startScale = 4f;
    public float endScale = 0.3f;

    [Header("Visual Offset")]
    public Vector3 spawnOffset = new Vector3(0f, 2f, 0f); // above the player

    private float timer = 0f;
    private bool active = false;

    public Action<bool, bool> onComplete; // (success, perfect)

    private KeyCode targetKey;
    private GameObject activeIndicator;
    private GameObject activeCircle;
    private GameObject activeHitMarker;

    private KeyCode[] player1Keys = { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow };
    private KeyCode[] player2Keys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    /// <summary>
    /// Start QTE above a specific player transform
    /// </summary>
    public void StartQTE(float qteDuration, int playerNumber, Transform playerTransform)
    {
        duration = qteDuration;
        timer = 0f;
        active = true;

        if (playerTransform == null)
        {
            Debug.LogError("[QTE] Player transform is null!");
            return;
        }

        Vector3 spawnPos = playerTransform.position + spawnOffset;

        // Pick random key
        KeyCode[] keys = playerNumber == 1 ? player1Keys : player2Keys;
        targetKey = keys[UnityEngine.Random.Range(0, keys.Length)];
        Debug.Log($"[QTE] Player {playerNumber} must press {targetKey}");

        // Spawn circle prefab
        if (circlePrefab != null)
            activeCircle = Instantiate(circlePrefab, spawnPos, Quaternion.identity);

        // Spawn hit marker prefab
        if (hitMarkerPrefab != null)
            activeHitMarker = Instantiate(hitMarkerPrefab, spawnPos, Quaternion.identity);

        // Set initial scale for circle
        if (activeCircle != null)
            activeCircle.transform.localScale = Vector3.one * startScale;

        // Spawn key indicator prefab dynamically
        GameObject prefabToSpawn = GetPrefabForKey(targetKey);
        if (prefabToSpawn != null)
        {
            Vector3 indicatorPos = spawnPos + Vector3.up * 0f; // slightly above circle
            activeIndicator = Instantiate(prefabToSpawn, indicatorPos, Quaternion.identity);
        }

        Debug.Log("[QTE] Started!");
    }

    void Update()
    {
        if (!active) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        if (activeCircle != null)
        {
            float currentScale = Mathf.Lerp(startScale, endScale, t);
            activeCircle.transform.localScale = Vector3.one * currentScale;

            // Only check the target key
            if (Input.GetKeyDown(targetKey))
            {
                float dist = Mathf.Abs(currentScale - endScale);
                bool perfect = dist <= perfectWindow;
                bool success = dist <= goodWindow;

                if (perfect)
                    Debug.Log($"[QTE] PERFECT! Distance to marker: {dist:F3}");
                else if (success)
                    Debug.Log($"[QTE] GOOD. Distance to marker: {dist:F3}");
                else
                    Debug.Log($"[QTE] MISS. Distance to marker: {dist:F3}");

                EndQTE(success, perfect);
                return;
            }
        }

        if (timer >= duration)
        {
            Debug.Log("[QTE] TIME OUT - MISS!");
            EndQTE(false, false);
        }
    }

    private void EndQTE(bool success, bool perfect)
    {
        if (activeIndicator != null) Destroy(activeIndicator);
        if (activeCircle != null) Destroy(activeCircle);
        if (activeHitMarker != null) Destroy(activeHitMarker);

        active = false;
        Debug.Log($"[QTE] Ended. Success={success}, Perfect={perfect}");
        onComplete?.Invoke(success, perfect);
    }

    private GameObject GetPrefabForKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow: return upArrowPrefab;
            case KeyCode.DownArrow: return downArrowPrefab;
            case KeyCode.LeftArrow: return leftArrowPrefab;
            case KeyCode.RightArrow: return rightArrowPrefab;
            case KeyCode.W: return wPrefab;
            case KeyCode.A: return aPrefab;
            case KeyCode.S: return sPrefab;
            case KeyCode.D: return dPrefab;
            default: return null;
        }
    }
}

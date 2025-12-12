#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageCameraMover : MonoBehaviour
{
    [Header("References")]
    public ProgressBar progressBar;
    public playerMovementScript player;
    public SpriteRenderer fadeOverlay; // optional fade overlay

    [Header("Stage Y Positions (Bottom → Top)")]
    public List<float> stageYPositions = new List<float> { 5f, 20f, 35f };

    [Header("Stage Thresholds (0–1)")]
    public List<float> stageThresholds = new List<float> { 0f, 0.33f, 0.66f };

    [Header("Player Spawn Points (same order as stages)")]
    public List<Transform> playerSpawnPoints = new List<Transform>();

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float fadeDuration = 0.5f;

    private int currentStageIndex = 0;
    private bool isTransitioning = false;

    // 🔔 Event for other systems (like minigames)
    public static System.Action OnCameraStageSwitched;

    public static int CurrentLevel = 0;

    private void Update()
    {
        if (progressBar == null || stageYPositions.Count == 0 || stageThresholds.Count != stageYPositions.Count)
            return;

        float value = progressBar.GetValue();

        int targetStage = 0;
        for (int i = 0; i < stageThresholds.Count; i++)
        {
            if (value >= stageThresholds[i])
                targetStage = i;
            else
                break;
        }

        if (targetStage != currentStageIndex && !isTransitioning)
        {
            StartCoroutine(HandleStageTransition(targetStage));
        }

        // Move camera smoothly toward target stage height
        float targetY = stageYPositions[targetStage];
        Vector3 currentPos = transform.position;
        float newY = Mathf.MoveTowards(currentPos.y, targetY, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(currentPos.x, newY, currentPos.z);
    }

    private IEnumerator HandleStageTransition(int newStage)
{
        isTransitioning = true;

    // 🔔 Notify minigame zones (or other listeners)
    CurrentLevel = newStage; 
    OnCameraStageSwitched?.Invoke();

    // Disable player movement
    if (player != null)
        player.DisableMovement();

    // Fade out
    if (fadeOverlay != null)
        yield return StartCoroutine(Fade(1f, fadeDuration));

    // ⏳ Wait 2 seconds before teleport
    yield return new WaitForSeconds(2f);

    // Move player to new spawn point (keep Z constant)
    if (player != null && newStage < playerSpawnPoints.Count && playerSpawnPoints[newStage] != null)
    {
        Vector3 targetPos = playerSpawnPoints[newStage].position;
        targetPos.z = -1f; // ✅ keep Z fixed
        player.transform.position = targetPos;
    }

    // Fade back in
    if (fadeOverlay != null)
        yield return StartCoroutine(Fade(0f, fadeDuration));

    // Re-enable movement
    if (player != null)
        player.EnableMovement();

    currentStageIndex = newStage;
    isTransitioning = false;
}


    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (fadeOverlay == null) yield break;

        Color color = fadeOverlay.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        fadeOverlay.color = color;
    }

    void SwitchToStage(int nextLevel)
    {
        CurrentLevel = nextLevel;
        OnCameraStageSwitched?.Invoke();
    }


    // --- Draw thresholds in Scene View ---
    private void OnDrawGizmos()
    {
        if (stageYPositions == null || stageThresholds == null || stageYPositions.Count != stageThresholds.Count)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < stageYPositions.Count; i++)
        {
            Vector3 pos = new Vector3(transform.position.x, stageYPositions[i], transform.position.z);
            Gizmos.DrawSphere(pos, 0.5f);

#if UNITY_EDITOR
            Handles.Label(pos + Vector3.right * 1f, $"Threshold: {stageThresholds[i]}");
#endif
        }
    }
}









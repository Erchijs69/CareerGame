#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

public class StageCameraMover : MonoBehaviour
{
    [Header("Progress Bar Reference")]
    public ProgressBar progressBar;

    [Header("Stage Y Positions (Bottom → Top)")]
    public List<float> stageYPositions = new List<float> { 5f, 20f, 35f };

    [Header("Stage Thresholds (0–1)")]
    public List<float> stageThresholds = new List<float> { 0f, 0.33f, 0.66f }; 

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private int currentStageIndex = 0;

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

        float targetY = stageYPositions[targetStage];
        Vector3 currentPos = transform.position;
        float newY = Mathf.MoveTowards(currentPos.y, targetY, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(currentPos.x, newY, currentPos.z);

        currentStageIndex = targetStage;
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







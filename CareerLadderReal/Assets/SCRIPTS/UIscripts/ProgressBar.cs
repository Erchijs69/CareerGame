using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProgressBar : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider progressSlider;

    // Store drain rates per system
    private Dictionary<object, float> activeDrainers = new Dictionary<object, float>();

    [Header("Default Settings")]
    public float baseGainAmount = 0.1f;

    // Debug/test speed
    public float manualGainSpeed = 0.5f;
    public float manualDrainSpeed = 0.5f;

    // --- New: pause toggle ---
    private bool isPaused = false;

    private void Start()
    {
        if (progressSlider == null)
            progressSlider = GetComponent<Slider>();

        progressSlider.value = 0f;
    }

    private void Update()
    {
        if (progressSlider == null)
            return;

        // Toggle pause on "I"
        if (Input.GetKeyDown(KeyCode.I))
            isPaused = !isPaused;

        if (!isPaused)
        {
            // --- Automatic draining ---
            float totalDrain = 0f;
            foreach (var rate in activeDrainers.Values)
                totalDrain += rate;

            if (totalDrain > 0f)
            {
                progressSlider.value -= totalDrain * Time.deltaTime;
                progressSlider.value = Mathf.Clamp01(progressSlider.value);
            }

            // --- Debug controls ---
            if (Input.GetKey(KeyCode.P))
            {
                progressSlider.value += manualGainSpeed * Time.deltaTime;
                progressSlider.value = Mathf.Clamp01(progressSlider.value);
            }

            if (Input.GetKey(KeyCode.O))
            {
                progressSlider.value -= manualDrainSpeed * Time.deltaTime;
                progressSlider.value = Mathf.Clamp01(progressSlider.value);
            }
        }
    }

    // --- Drain Management ---

    public void StartDraining(object requester, float drainSpeed)
    {
        if (!activeDrainers.ContainsKey(requester))
            activeDrainers.Add(requester, drainSpeed);
        else
            activeDrainers[requester] = drainSpeed;
    }

    public void StopDraining(object requester)
    {
        if (activeDrainers.ContainsKey(requester))
            activeDrainers.Remove(requester);
    }

    public void AddProgress(object requester, float gainAmount)
    {
        progressSlider.value += gainAmount;
        progressSlider.value = Mathf.Clamp01(progressSlider.value);
    }

    public void ResetProgress()
    {
        progressSlider.value = 0f;
    }

    public float GetValue() => progressSlider.value;

    // Optional: public getter to check pause state
    public bool IsPaused() => isPaused;
}










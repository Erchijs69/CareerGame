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

        // Sum all active drains
        float totalDrain = 0f;
        foreach (var rate in activeDrainers.Values)
            totalDrain += rate;

        if (totalDrain > 0f)
        {
            progressSlider.value -= totalDrain * Time.deltaTime;
            progressSlider.value = Mathf.Clamp01(progressSlider.value);
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
}








using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider progressSlider;

    [Header("Default Settings")]
    public float drainSpeed = 0.05f;  // Default drain rate
    public float gainAmount = 0.1f;   // Default gain amount

    private bool shouldDrain = false;

    private void Start()
    {
        if (progressSlider == null)
            progressSlider = GetComponent<Slider>();

        progressSlider.value = 0f; // Start empty
    }

    private void Update()
    {
        if (shouldDrain)
        {
            progressSlider.value -= drainSpeed * Time.deltaTime;
            progressSlider.value = Mathf.Clamp01(progressSlider.value);
        }
    }

    public void StartDraining() => shouldDrain = true;
    public void StopDraining() => shouldDrain = false;

    public void AddProgress()
    {
        progressSlider.value += gainAmount;
        progressSlider.value = Mathf.Clamp01(progressSlider.value);
    }

    public void SetRates(float newDrainSpeed, float newGainAmount)
    {
        drainSpeed = newDrainSpeed;
        gainAmount = newGainAmount;
    }

    public float GetValue() => progressSlider.value;
}






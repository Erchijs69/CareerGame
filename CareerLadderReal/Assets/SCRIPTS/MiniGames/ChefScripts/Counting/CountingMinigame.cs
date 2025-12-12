using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CountingMinigame : MonoBehaviour
{
    [Header("Dependencies")]
    public MinigameZone minigameZone;

    [Header("Level Setup")]
    public int levelIndex;
    public int LevelIndex => levelIndex;

    [Header("UI Elements")]
    public GameObject uiPoster;
    public Button approveButton;
    public TMP_InputField[] inputFields;
    public TextMeshProUGUI[] ingredientLabels;

    [Header("Item Spawn Settings")]
    public Transform[] spawnPoints;
    public GameObject[] ingredientPrefabs;

    [Header("Progress Bar (shared)")]
    public ProgressBar progressBar;
    public float countingGainAmount = 0.1f;
    public float countingFailPenalty = -0.05f;
    public float drainSpeedAfterCooldown = 0.02f; // New: how fast progress bar drains if minigame not completed

    public float inGameDrainSpeed = 0.005f;

    [Header("Cooldown Settings")]
    public TMP_Text[] countdownTexts; // UI text to display cooldown
    public float cooldownDuration = 10f;

    private Dictionary<string, int> correctCounts = new Dictionary<string, int>();
    private List<GameObject> spawnedItems = new List<GameObject>();

    private bool active = false;
    private bool onCooldown = false;

    private bool isActive = false;
    public bool InMinigame => isActive;

    private Coroutine cooldownCoroutine;

    void OnEnable()
    {
        StageCameraMover.OnCameraStageSwitched += HandleStageSwitch;
    }

    void OnDisable()
    {
        StageCameraMover.OnCameraStageSwitched -= HandleStageSwitch;
    }


    private void HandleStageSwitch()
{
    if (StageCameraMover.CurrentLevel == levelIndex)
        ActivateMinigame();
    else
        DeactivateMinigame();
}


    void Start()
{
    if (approveButton != null)
        approveButton.onClick.AddListener(CheckAnswers);

    uiPoster.SetActive(false);

    foreach (var text in countdownTexts)
        if (text != null) text.gameObject.SetActive(false);

    if (StageCameraMover.CurrentLevel == levelIndex)
        ActivateMinigame();
    else
        DeactivateMinigame();
}


    void Update()
{
    if (!isActive) return; // <--- IMPORTANT

    // Enter minigame only if cooldown is over
    if (minigameZone != null && minigameZone.InMinigame && !active && !onCooldown)
    {
        StartMinigame();
    }

    if (active && !minigameZone.InMinigame)
    {
        EndMinigame();
    }
}


    void StartMinigame()
{
    active = true;
    uiPoster.SetActive(true);
    GenerateItems();

    // Start slow draining while minigame is active
    if (progressBar != null)
        progressBar.StartDraining(this, inGameDrainSpeed);
}



    void EndMinigame()
    {
        active = false;
        uiPoster.SetActive(false);
        ClearSpawnedItems();
        ClearInputs();

        // Only start normal draining if cooldown is over
        if (!onCooldown && progressBar != null)
            progressBar.StartDraining(this, drainSpeedAfterCooldown);
    }

private void ActivateMinigame()
{
    if (isActive) return;
    isActive = true;

    // Start draining slowly while player is NOT in the minigame zone
    if (!active && !onCooldown && progressBar != null)
        progressBar.StartDraining(this, drainSpeedAfterCooldown);
}

private void DeactivateMinigame()
{
    if (!isActive) return;
    isActive = false;

    // Stop progress drain completely when the minigame level is not active
    if (progressBar != null)
        progressBar.StopDraining(this);

    // Cancel cooldown if running
    if (cooldownCoroutine != null)
        StopCoroutine(cooldownCoroutine);

    // Remove items & UI so they don't stay floating offstage
    ClearSpawnedItems();
    ClearInputs();
    uiPoster.SetActive(false);

    // Reset flags
    active = false;
    onCooldown = false;

    // Hide countdown numbers
    foreach (var text in countdownTexts)
        if (text != null) text.gameObject.SetActive(false);
}






    void GenerateItems()
    {
        ClearSpawnedItems();
        correctCounts.Clear();

        var points = spawnPoints.OrderBy(x => Random.value).ToList();
        int itemCount = Random.Range(4, 11);

        for (int i = 0; i < itemCount; i++)
        {
            var ingredient = ingredientPrefabs[Random.Range(0, ingredientPrefabs.Length)];
            var spawn = points[i % points.Count];

            GameObject obj = Instantiate(ingredient, spawn.position, Quaternion.identity, transform);
            spawnedItems.Add(obj);

            string name = ingredient.name;
            if (!correctCounts.ContainsKey(name))
                correctCounts[name] = 0;

            correctCounts[name]++;
        }

        for (int i = 0; i < ingredientLabels.Length; i++)
            ingredientLabels[i].text = i < ingredientPrefabs.Length ? ingredientPrefabs[i].name : "";

        ClearInputs();
    }

    void ClearInputs()
    {
        foreach (var field in inputFields)
            field.text = "";
    }

    void ClearSpawnedItems()
    {
        foreach (var obj in spawnedItems)
            if (obj != null) Destroy(obj);
        spawnedItems.Clear();
    }

    void CheckAnswers()
    {
        bool allCorrect = true;

        for (int i = 0; i < ingredientPrefabs.Length; i++)
        {
            string ingredientName = ingredientPrefabs[i].name;
            int correct = correctCounts.ContainsKey(ingredientName) ? correctCounts[ingredientName] : 0;

            if (i >= inputFields.Length) continue;

            string input = inputFields[i].text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                if (correct != 0) allCorrect = false;
                continue;
            }

            if (!int.TryParse(input, out int userCount) || userCount != correct)
                allCorrect = false;
        }

        if (allCorrect)
        {
            Debug.Log("✅ Correct! Starting cooldown.");

            if (progressBar != null)
            {
                progressBar.AddProgress(this, countingGainAmount);
                progressBar.StopDraining(this);
            }

            if (minigameZone != null)
            {
                Collider2D col = minigameZone.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }

            minigameZone.ExitMinigame();
            EndMinigame();

            if (cooldownCoroutine != null)
                StopCoroutine(cooldownCoroutine);

            cooldownCoroutine = StartCoroutine(CooldownCoroutine(cooldownDuration));
        }
        else
        {
            Debug.Log("❌ Incorrect, try again!");
            if (progressBar != null)
                progressBar.AddProgress(this, countingFailPenalty);

            GenerateItems();
        }
    }

    private IEnumerator CooldownCoroutine(float duration)
{
    onCooldown = true;

    // Show countdown UI
    foreach (var text in countdownTexts)
        if (text != null) text.gameObject.SetActive(true);

    float remaining = duration;
    while (remaining > 0)
    {
        string displayTime = Mathf.CeilToInt(remaining).ToString();
        foreach (var text in countdownTexts)
            if (text != null) text.text = displayTime;

        // Always stop draining during cooldown
        if (progressBar != null)
            progressBar.StopDraining(this);

        remaining -= Time.deltaTime;
        yield return null;
    }

    onCooldown = false;

    // Hide countdown UI
    foreach (var text in countdownTexts)
        if (text != null) text.gameObject.SetActive(false);

    // Re-enable minigame zone
    if (minigameZone != null)
    {
        Collider2D col = minigameZone.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    // Start slowly draining only if minigame is not active
    if (!active && progressBar != null)
        progressBar.StartDraining(this, drainSpeedAfterCooldown);
}

}





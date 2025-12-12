using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class Dish
{
    public string name;
    public Sprite image;
    public string[] requiredKeywords;
    [HideInInspector] public List<string> generatedIngredients = new List<string>();
}

public class PricingMinigame : MonoBehaviour
{
    [Header("Dependencies")]
    public MinigameZone minigameZone;

    [Header("Level Setup")]
    public int levelIndex;
    public int LevelIndex => levelIndex;

    [Header("UI Elements")]
    public GameObject uiPoster;
    public TMP_Text dishNameText;
    public Image dishImage;
    public Transform stickyNotesParent;
    public GameObject stickyEntryPrefab;
    public TMP_InputField priceInputField;
    public Button approveButton;

    [Header("Progress Bar (shared)")]
    public ProgressBar progressBar;
    public float successGainAmount = 0.1f;
    public float failPenalty = -0.05f;

    [Header("Dish Setup")]
    public Dish[] allDishes; // assign 4 dishes in inspector

    [Header("Ingredients")]
    public string[] fixedIngredients = new string[]
    {
        "Tomato","Onion","Carrot","Potato","Garlic",
        "Chicken Breast","Beef Steak","Salmon Fillet","Apple","Banana",
        "Lettuce","Cucumber","Flour","Sugar","Salt"
    };

    // Runtime variables
    private bool active = false;
    private bool isActive = false;
    private bool completed = false;
    private Dish selectedDish;
    private Dictionary<string, float> ingredientPrices = new Dictionary<string, float>();
    private float correctTotal = 0f;
    private List<GameObject> spawnedStickyEntries = new List<GameObject>();

    public bool InMinigame => isActive;
    public bool Completed => completed;

    void OnEnable() => StageCameraMover.OnCameraStageSwitched += HandleStageSwitch;
    void OnDisable() => StageCameraMover.OnCameraStageSwitched -= HandleStageSwitch;

    void Start()
    {
        if (approveButton != null)
            approveButton.onClick.AddListener(OnSubmit);

        uiPoster.SetActive(false);

        if (StageCameraMover.CurrentLevel == levelIndex)
            ActivateMinigame();
        else
            DeactivateMinigame();
    }

    void Update()
    {
        if (!isActive || completed) return;

        if (minigameZone != null && minigameZone.InMinigame && !active)
            StartMinigame();

        if (active && (minigameZone == null || !minigameZone.InMinigame))
            EndMinigame();
    }

    private void HandleStageSwitch()
    {
        if (StageCameraMover.CurrentLevel == levelIndex)
            ActivateMinigame();
        else
            DeactivateMinigame();
    }

    private void ActivateMinigame()
    {
        if (isActive) return;
        isActive = true;

        if (selectedDish == null)
            GenerateRound();
    }

    private void DeactivateMinigame()
    {
        if (!isActive) return;
        isActive = false;

        active = false;
        uiPoster.SetActive(false);
    }

    private void StartMinigame()
    {
        if (completed || active) return;

        active = true;
        uiPoster.SetActive(true);
        priceInputField.text = "";
    }

    private void EndMinigame()
    {
        active = false;
        uiPoster.SetActive(false);
    }

    private void GenerateRound()
    {
        ClearAllEntries();
        ingredientPrices.Clear();
        correctTotal = 0f;

        // Assign random prices for all ingredients
        foreach (var ing in fixedIngredients)
        {
            float price = Random.Range(1f, 10f);
            price = Mathf.Round(price * 10f) / 10f; // 1 decimal place
            ingredientPrices[ing] = price;
        }

        // Generate ingredients for all dishes
        foreach (var dish in allDishes)
        {
            dish.generatedIngredients.Clear();
            dish.generatedIngredients.AddRange(dish.requiredKeywords);

            int extraCount = Random.Range(0, 3); // 0-2 extra
            for (int i = 0; i < extraCount; i++)
            {
                string extra = fixedIngredients[Random.Range(0, fixedIngredients.Length)];
                if (!dish.generatedIngredients.Contains(extra))
                    dish.generatedIngredients.Add(extra);
            }

            // shuffle
            // shuffle ingredients
// shuffle ingredients
for (int i = 0; i < dish.generatedIngredients.Count; i++)
{
    int j = Random.Range(i, dish.generatedIngredients.Count);
    string temp = dish.generatedIngredients[i];
    dish.generatedIngredients[i] = dish.generatedIngredients[j];
    dish.generatedIngredients[j] = temp;
}


        }

        // Select one dish randomly
        selectedDish = allDishes[Random.Range(0, allDishes.Length)];

        // Update UI
        dishNameText.text = selectedDish.name;
        dishImage.sprite = selectedDish.image;

        // Spawn sticky notes for selected dish
        for (int i = 0; i < selectedDish.generatedIngredients.Count; i++)
        {
            GameObject entry = Instantiate(stickyEntryPrefab, stickyNotesParent);
            entry.transform.localPosition = new Vector3(0f, i * 35f, 0f);
            entry.transform.Find("Name")?.GetComponent<TMP_Text>().text = selectedDish.generatedIngredients[i];
            spawnedStickyEntries.Add(entry);

            correctTotal += ingredientPrices[selectedDish.generatedIngredients[i]];
        }

        correctTotal = Mathf.Round(correctTotal * 10f) / 10f;
        Debug.Log($"The complete sum is {correctTotal}");
    }

    private void ClearAllEntries()
    {
        foreach (var go in spawnedStickyEntries)
            if (go) Destroy(go);

        spawnedStickyEntries.Clear();
    }

    private void OnSubmit()
    {
        if (completed) return;

        if (!float.TryParse(priceInputField.text, out float playerValue))
        {
            HandleIncorrect();
            return;
        }

        if (Mathf.Abs(playerValue - correctTotal) < 0.01f)
            HandleCorrect();
        else
            HandleIncorrect();
    }

    private void HandleCorrect()
    {
        completed = true;
        progressBar?.AddProgress(this, successGainAmount);

        active = false;
        uiPoster.SetActive(false);

        minigameZone?.ForceExitMinigame();

        if (minigameZone != null)
        {
            var col = minigameZone.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    private void HandleIncorrect()
    {
        progressBar?.AddProgress(this, failPenalty);
        // Do NOT regenerate; keep ingredients the same
    }
}


















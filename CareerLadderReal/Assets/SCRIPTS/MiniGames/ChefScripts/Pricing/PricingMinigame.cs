using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PricingMinigame : MonoBehaviour
{
    [Header("Dependencies")]
    public MinigameZone minigameZone;

    [Header("Level Setup")]
    public int levelIndex;
    public int LevelIndex => levelIndex;

    [Header("UI Elements")]
    public GameObject uiPoster;
    public Transform leftListParent;
    public GameObject leftEntryPrefab;
    public Transform[] stickyNoteParents;
    public GameObject stickyEntryPrefab;
    public TMP_InputField priceInputField;
    public Button approveButton;

    [Header("Dish UI (for 4 dishes)")]
    public TMP_Text[] dishNameTexts;   // 4 text fields for names
    public Image[] dishImages;          // 4 image fields for dish images
    public Sprite steakAndPotatoesSprite;
    public Sprite caesarSaladSprite;
    public Sprite bolognesePastaSprite;
    public Sprite applePieSprite;

    [Header("Progress Bar (shared)")]
    public ProgressBar progressBar;
    public float successGainAmount = 0.1f;
    public float failPenalty = -0.05f;

    // Runtime variables
    private bool active = false;       // inside minigame
    private bool isActive = false;     // level switched to this minigame
    private bool completed = false;    // minigame completed once

    private List<GameObject> spawnedLeftEntries = new List<GameObject>();
    private List<GameObject> spawnedStickyEntries = new List<GameObject>();
    private Dictionary<string, float> ingredientPrices = new Dictionary<string, float>();
    private float correctTotal = 0f;

    private readonly string[] fixedIngredients = new string[]
    {
        "Tomato","Onion","Carrot","Potato","Garlic",
        "Chicken Breast","Beef Steak","Salmon Fillet","Apple","Banana",
        "Lettuce","Cucumber","Flour","Sugar","Salt"
    };

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

        // Enter minigame when player is in the zone
        if (minigameZone != null && minigameZone.InMinigame && !active)
            StartMinigame();

        // Exit minigame if player leaves
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

        if (ingredientPrices.Count == 0)
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

        // Assign random float prices (1.0–10.0, 1 decimal place)
        foreach (var ing in fixedIngredients)
        {
            float price = Random.Range(1f, 10f);
            price = Mathf.Round(price * 10f) / 10f;
            ingredientPrices[ing] = price;
        }

        // Populate sticky notes
        List<string> shuffled = new List<string>(fixedIngredients);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int j = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        int index = 0;
        for (int note = 0; note < 3; note++)
        {
            Transform parent = note < stickyNoteParents.Length ? stickyNoteParents[note] : transform;

            for (int slot = 0; slot < 5; slot++)
            {
                if (index >= shuffled.Count) break;
                string ing = shuffled[index];

                GameObject entry = Instantiate(stickyEntryPrefab, parent);
                spawnedStickyEntries.Add(entry);
                entry.transform.localPosition = new Vector3(0f, slot * 35f, 0f);

                entry.transform.Find("Name")?.GetComponent<TMP_Text>().SetText(ing);
                entry.transform.Find("Price")?.GetComponent<TMP_Text>().SetText(ingredientPrices[ing].ToString());
                index++;
            }
        }

        // ------------------------
        // Generate left list with 4 dishes and required keywords
        // ------------------------
        string[] dishNames = { "Steak And Potatoes", "Caesar Salad", "Bolognese Pasta", "ApplePie" };
        Sprite[] dishSprites = { steakAndPotatoesSprite, caesarSaladSprite, bolognesePastaSprite, applePieSprite };
        string[][] dishKeywords = new string[][]
        {
            new string[] { "Beef Steak", "Potato" },
            new string[] { "Chicken Breast", "Lettuce" },
            new string[] { "Tomato", "Onion", "Garlic", "Carrot" },
            new string[] { "Apple", "Flour", "Sugar" }
        };

        for (int i = 0; i < 4; i++)
        {
            if (i < dishNameTexts.Length && dishNameTexts[i] != null)
                dishNameTexts[i].text = dishNames[i];

            if (i < dishImages.Length && dishImages[i] != null)
                dishImages[i].sprite = dishSprites[i];

            // Include required keywords and random extras (total 5 ingredients per dish)
            List<string> possibleExtras = new List<string>(fixedIngredients);
            List<string> selectedIngredients = new List<string>(dishKeywords[i]);

            while (selectedIngredients.Count < 5)
            {
                string randIng = possibleExtras[Random.Range(0, possibleExtras.Count)];
                if (!selectedIngredients.Contains(randIng))
                    selectedIngredients.Add(randIng);
            }

            // Shuffle
            for (int j = 0; j < selectedIngredients.Count; j++)
            {
                int k = Random.Range(j, selectedIngredients.Count);
                (selectedIngredients[j], selectedIngredients[k]) = (selectedIngredients[k], selectedIngredients[j]);
            }

            // Spawn entries in left list
            GameObject entry = Instantiate(leftEntryPrefab, leftListParent);
            spawnedLeftEntries.Add(entry);
            entry.transform.localPosition = new Vector3(0f, i * 35f, 0f);
            entry.transform.Find("Name")?.GetComponent<TMP_Text>().SetText(dishNames[i] + ": " + string.Join(", ", selectedIngredients));

            // Add prices to total for correct answer
            foreach (var ing in selectedIngredients)
                if (ingredientPrices.TryGetValue(ing, out float p))
                    correctTotal += p;
        }

        correctTotal = Mathf.Round(correctTotal * 10f) / 10f;
        Debug.Log($"The complete sum is {correctTotal}");
    }

    private void ClearAllEntries()
    {
        foreach (var go in spawnedLeftEntries) if (go) Destroy(go);
        foreach (var go in spawnedStickyEntries) if (go) Destroy(go);

        spawnedLeftEntries.Clear();
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

        // Disable collider so it can't be entered again
        if (minigameZone != null)
        {
            var col = minigameZone.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    private void HandleIncorrect()
    {
        progressBar?.AddProgress(this, failPenalty);
        // Do NOT regenerate items
    }
}



















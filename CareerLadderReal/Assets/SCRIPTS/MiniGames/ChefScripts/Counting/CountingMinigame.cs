using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class CountingMinigame : MonoBehaviour
{
    [Header("Dependencies")]
    public MinigameZone minigameZone;

    [Header("UI Elements")]
    public GameObject uiPoster;                    // The canvas poster with ingredient names and input boxes
    public Button approveButton;
    public TMP_InputField[] inputFields;           // One per ingredient
    public TextMeshProUGUI[] ingredientLabels;     // Ingredient names beside inputs

    [Header("Item Spawn Settings")]
    public Transform[] spawnPoints;                // 10 spawn points
    public GameObject[] ingredientPrefabs;         // Prefabs for ingredients (assign images here)

    private Dictionary<string, int> correctCounts = new Dictionary<string, int>();
    private List<GameObject> spawnedItems = new List<GameObject>();

    private bool active = false;

    void Start()
    {
        if (approveButton != null)
            approveButton.onClick.AddListener(CheckAnswers);

        uiPoster.SetActive(false);
    }

    void Update()
    {
        // Only enter when player has entered minigame from MinigameZone
        if (minigameZone != null && minigameZone.InMinigame && !active)
        {
            StartMinigame();
        }

        // If minigame exits externally (camera switch, etc.)
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
    }

    void EndMinigame()
    {
        active = false;
        uiPoster.SetActive(false);
        ClearSpawnedItems();
        ClearInputs();
    }

    void GenerateItems()
    {
        ClearSpawnedItems();
        correctCounts.Clear();

        // Shuffle spawn points
        var points = spawnPoints.OrderBy(x => Random.value).ToList();

        // Random number of items (e.g., 4–10 items)
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

        // Set all ingredient labels on the poster
        for (int i = 0; i < ingredientLabels.Length; i++)
        {
            if (i < ingredientPrefabs.Length)
                ingredientLabels[i].text = ingredientPrefabs[i].name;
            else
                ingredientLabels[i].text = "";
        }

        // Clear all input fields
        ClearInputs();
    }

    void ClearInputs()
    {
        foreach (var field in inputFields)
        {
            field.text = "";
        }
    }

    void ClearSpawnedItems()
    {
        foreach (var obj in spawnedItems)
        {
            if (obj != null)
                Destroy(obj);
        }
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
                if (correct != 0)
                    allCorrect = false;
                continue;
            }

            if (!int.TryParse(input, out int userCount) || userCount != correct)
                allCorrect = false;
        }

        if (allCorrect)
        {
            Debug.Log("✅ Correct! Exiting minigame.");
            minigameZone.ExitMinigame();
            EndMinigame();
        }
        else
        {
            Debug.Log("❌ Incorrect, try again!");
            GenerateItems(); // Restart with a new random layout
        }
    }
}


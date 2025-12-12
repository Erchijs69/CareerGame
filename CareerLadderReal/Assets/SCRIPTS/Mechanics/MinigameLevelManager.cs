using UnityEngine;
using System.Linq;

public class MinigameLevelManager : MonoBehaviour
{
    private IMinigame[] allMinigames;

    private void Awake()
    {
        allMinigames = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<IMinigame>()
            .ToArray();
    }

    private void OnEnable()
    {
        StageCameraMover.OnCameraStageSwitched += OnLevelChanged;
    }

    private void OnDisable()
    {
        StageCameraMover.OnCameraStageSwitched -= OnLevelChanged;
    }

    private void OnLevelChanged()
    {
        int activeLevel = StageCameraMover.CurrentLevel;

        foreach (var mg in allMinigames)
        {
            if (mg.LevelIndex == activeLevel)
                mg.EnableMinigame();
            else
                mg.DisableAndResetMinigame();
        }
    }
}


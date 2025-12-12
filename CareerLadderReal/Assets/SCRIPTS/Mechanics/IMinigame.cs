using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMinigame
{
    int LevelIndex { get; }

    /// <summary>
    /// Called when this minigame becomes active (its level is entered)
    /// </summary>
    void EnableMinigame();

    /// <summary>
    /// Called when leaving this minigame's level
    /// Should stop timers, spawning, drains, etc.
    /// </summary>
    void DisableAndResetMinigame();
}


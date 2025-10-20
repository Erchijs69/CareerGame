using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] public Toggle toggleFullscreen;

    public void ToggleFullscreen()
    {
        Screen.fullScreen = toggleFullscreen.isOn;
    }
}

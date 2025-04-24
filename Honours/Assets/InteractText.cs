using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InteractText : MonoBehaviour
{
    public TextMeshProUGUI promptText; // UI text for "E" or "A"

    private PlayerInput playerInput;

    void Start()
    {
        InputSystem.onAnyButtonPress.Call(OnAnyInput);
        UpdatePrompts(); // fallback if no input yet
    }

    void OnAnyInput(InputControl control)
    {
        UpdatePrompts(control.device);
    }

    void UpdatePrompts(InputDevice device = null)
    {
        if (device == null)
            device = Keyboard.current;

        bool isGamepad = device is Gamepad;

        if (promptText != null)
        {
            promptText.text = isGamepad ? "B" : "E";
        }
    }
}
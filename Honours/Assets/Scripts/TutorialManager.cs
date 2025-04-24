using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class TutorialManager : MonoBehaviour
{

    [Header("Text Components")]
    public TextMeshProUGUI humanControlText;
    public TextMeshProUGUI wolfControlText;

    string humanKeyboardText =
        "MOUSE - AIM GUN\n" +
        "LEFT MOUSE BUTTON OR F - FIRE\n" +
        "SHIFT - DASH \n" +
        "SPACE - CHANGE FORM";

    string wolfKeyboardText =
        "LEFT MOUSE BUTTON - BITE ATTACK\n" +
        "SHIFT - KNIFE ATTACK";

    string humanControllerText =
        "RIGHT STICK - AIM\n" +
        "RT - FIRE\n" +
        "RB - DASH\n" +
        "Y - CHANGE FORM";

    string wolfControllerText =
        "RT - BITE ATTACK\n" +
        "B OR RB - KNIFE ATTACK";

    void Start()
    {
        InputSystem.onAnyButtonPress.Call(OnAnyInput);
        UpdatePrompts(); 
    }

    void OnAnyInput(InputControl control)
    {
        UpdatePrompts(control.device);
    }

    void UpdatePrompts(InputDevice device = null)
    {
        // Displays appropriate prompt based on if the player is using contoller or keyboard input
        if (device == null) device = Keyboard.current;

        bool isGamepad = device is Gamepad;

        if (humanControlText != null)
            humanControlText.text = isGamepad ? humanControllerText : humanKeyboardText;

        if (wolfControlText != null)
            wolfControlText.text = isGamepad ? wolfControllerText : wolfKeyboardText;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI text;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.8f, 0.8f, 0.8f);     
    public Color pressedColor = new Color(0.53f, 0.53f, 0.53f); 

    bool isHovered = false;

    void Reset()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        text.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        text.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isHovered)
        {
            text.color = hoverColor;
        }
        else
        {
            text.color = normalColor;
        }
    }

}

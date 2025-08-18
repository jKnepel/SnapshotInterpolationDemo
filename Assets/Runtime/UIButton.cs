using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public event Action ClickStart;
    public event Action ClickEnd;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        ClickStart?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ClickEnd?.Invoke();
    }
}
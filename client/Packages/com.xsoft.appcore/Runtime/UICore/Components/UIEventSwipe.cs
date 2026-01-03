using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIEventSwipe : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Vector2? last;
    private Vector2? start;

    public SwipeEvent OnSwiping { get; } = new();

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (last == null) return;
        var diff = last.Value - eventData.position;
        last = eventData.position;

        OnSwiping?.Invoke(diff);
        // Debug.Log(diff);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        last = start = eventData.position;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        last = start = null;
    }

    [Serializable]
    public class SwipeEvent : UnityEvent<Vector2>
    {
    }
}
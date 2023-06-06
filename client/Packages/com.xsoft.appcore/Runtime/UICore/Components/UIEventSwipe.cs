using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIEventSwipe : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [System.Serializable] public class SwipeEvent : UnityEvent<Vector2> { }
    private Vector2? start;
    private Vector2? last;

    public SwipeEvent OnSwiping { private set; get; } = new SwipeEvent();

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
        last= start = eventData.position;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        last= start = null;
    }
}

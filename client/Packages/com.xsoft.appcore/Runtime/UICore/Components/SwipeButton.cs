using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SwipeButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public bool interactable = true;

    public float threshold = 100f;
    private Vector2? _last;

    private Vector2? _startPosition;

    public ButtonSwipeEvent OnSwipeClickEvent { get; } = new();

    public ButtonSwipeEvent OnDragging { get; } = new();

    public UnityEvent OnSwipeStarted { get; } = new();

    public UnityEvent OnSwipeEnd { get; } = new();

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (!interactable) return;
        if (!_startPosition.HasValue) return;
        _last = eventData.position - _startPosition.Value;
        OnDragging.Invoke(_last);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;
        _startPosition = eventData.position;
        OnSwipeStarted.Invoke();
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;
        var dir = _last;
        if (dir != null && dir.Value.sqrMagnitude < threshold) dir = null;

        _last = _startPosition = null;
        OnSwipeClickEvent.Invoke(dir);
        OnSwipeEnd.Invoke();
    }

    public class ButtonSwipeEvent : UnityEvent<Vector2?>
    {
    }
}
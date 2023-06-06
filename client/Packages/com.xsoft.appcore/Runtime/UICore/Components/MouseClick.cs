using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMouseClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool IsDown = false;
    private float time = -1f;

    public bool CheckMove = false;
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        time = Time.time;
        IsDown = true;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (IsDown)
        {
            if (CheckMove && eventData.IsPointerMoving()) return;

            if (Time.time - time < 1f)
            {
                eventData.Use();
                DoClick();
            }
        }
        IsDown = false;
    }

    private void DoClick()
    {
        OnClick?.Invoke(userState);
    }

    public Action<object> OnClick;

    public object userState;
}

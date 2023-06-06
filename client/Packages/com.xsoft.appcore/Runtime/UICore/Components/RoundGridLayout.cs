using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundGridLayout : LayoutGroup
{

    public Vector2 cellSize = new Vector2(100,100);
    public enum StartAxis
    {
        Top, Bottom, Left, Right
    }

    public bool preserve = false;
    public StartAxis axis = StartAxis.Bottom;
    [Range(0,180)]
    public float childAngle = 30;
    public float radius = 200;

    public override void CalculateLayoutInputVertical()
    {
        //CalRowAndCol();
        SetLayoutInputForAxis(100, 100, -1, 1);
    }

    public override void SetLayoutHorizontal()
    {
        DoLayout();
    }

    public override void SetLayoutVertical()
    {
        DoLayout();
    }

    private void DoLayout()
    {
        foreach (var i in rectChildren)
        {
            m_Tracker.Add(this, i,
                       DrivenTransformProperties.Anchors |
                       DrivenTransformProperties.AnchoredPosition |
                       DrivenTransformProperties.SizeDelta);

            i.anchorMin = Vector2.up;
            i.anchorMax = Vector2.up;
            i.sizeDelta = cellSize;
        }

        float startA = 0;
        switch (axis)
        {
            case StartAxis.Bottom:
                startA = 0;
                break;
            case StartAxis.Left:
                startA = 270;
                break;
            case StartAxis.Right:
                startA = 90;
                break;
            case StartAxis.Top:
            default:
                startA = 180;
                break;
        }

        var forward = Vector3.forward;
        var angle = startA;

        for (var index = 1; index <= rectChildren.Count; index++)
        {
            int x = 0;
            int y = 0;

            var q = Quaternion.Euler(0, angle, 0);
            var pos = q * forward * radius;
            x = (int)pos.x;
            y = (int)pos.z;

            if (!preserve)
                angle += this.childAngle;
            else
                angle -= this.childAngle;

            SetChildAlongAxis(rectChildren[index - 1], 0, x, cellSize.x);
            SetChildAlongAxis(rectChildren[index - 1], 1, y, cellSize.y);
        }
    }
}

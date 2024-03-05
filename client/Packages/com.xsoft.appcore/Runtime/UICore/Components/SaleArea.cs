using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class SaleArea : MonoBehaviour
{
    
    // Start is called before the first frame update
    private void Start()
    {
        if (!(UUIManager.S.Ratio > 1.9f)) return;
        var rect = GetComponent<RectTransform>();
        var offset = Screen.width * 0.06f;
        /*offsetMin 是vector2(left,bottom);
             *offsetMax 是vector2(right,top);*/
        rect.offsetMin = new Vector2(offset, 0);
        rect.offsetMax = new Vector2(-offset, 0);
    }
}

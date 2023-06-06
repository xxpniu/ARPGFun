using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaleArea : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (UUIManager.S.Ratio > 2)
        {
            var rect = GetComponent<RectTransform>();
            /*offsetMin 是vector2(left,bottom);
             *offsetMax 是vector2(right,top);*/
            rect.offsetMin = new Vector2(100, 0);
            rect.offsetMax = new Vector2(-100, 0);
        }
    }
}

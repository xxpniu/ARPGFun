using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof( Scrollbar))]
public class AutoValueScrollbar : MonoBehaviour {


    private Scrollbar bar;
	
    void Awake()
    {
        bar = GetComponent<Scrollbar>();
    }

    public void ResetValue(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(RunBar(duration));
    }

    private IEnumerator RunBar(float duration)
    {
        var start =Time.time;
        bar.size = 0;
        yield return null;
        while (Time.time - start < duration)
        {
            bar.size = (Time.time - start) / duration;
            yield return null;
        }
        bar.size = 1;
        yield return null;
    }
}

using App.Core.UICore.Utility;
using UnityEngine;

public class MainData : MonoBehaviour
{
    public Transform[] pos;

    // Use this for initialization
    private void Start()
    {
        foreach (var i in pos) i.ActiveSelfObject(false);
    }
}